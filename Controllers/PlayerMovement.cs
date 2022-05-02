using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using pokeCopy;
using System.Linq;

namespace pokeCopy
{
    public class PlayerMovement : MonoBehaviour, ISavable, INamable
    {
        [SerializeField] string playerName;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float runSpeed = 7f;

        //float speed;
        readonly float offset = -0.3f;

        [SerializeField] Rigidbody2D rb;

        [SerializeField] Animator animator;
        [SerializeField] Sprite battleSprite;

        Vector2 movement;

        bool isMoving;
        bool isRunning;

        PlayerInputActions inputActions;
        public PlayerInputActions InputActions { get => inputActions; }
        public Sprite BattleSprite { get => battleSprite; }
        public bool IsMoving => isMoving;

        public string InGameName => playerName;

        public string Name { get => playerName; }

        //    [SerializeField] bool isNorm = true;


        public void Awake()
        {

            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.Player != this)
                    Destroy(this.gameObject);


            }


            //DontDestroyOnLoad(gameObject);


            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
                Debug.Log("New input");
            }
            Debug.Log("Player woke");

            inputActions.PlayerDigital.Enable();
            // inputActions.Player.Move.performed += MoveAction;

            //Debug.Log(ExpTable.GetSize());
            // Debug.Log(ExpTable.GetNumGrothTypes());

        }
        public void Start()
        {
            SnapPositionToGrid(transform.position);
        }

        public void SnapPositionToGrid(Vector3 position)
        {
            animator.SetBool("IsMoving", isMoving);

            while (!IsWakable(position))
            {
                --position.x;
                --position.y;

            }

            position.x = Mathf.Floor(position.x) + .5f;
            position.y = Mathf.Floor(position.y) + .5f + offset;
            transform.position = position;
        }


        public void OnEnable()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsBattleAScene)
            {
                var d = GameManager.Instance.BattleSceneData.RetriveDirection();
                animator.SetFloat("Horizontal", d.x);
                animator.SetFloat("Vertical", d.y);

                transform.position = GameManager.Instance.BattleSceneData.ReturnPosition();
                GameManager.Instance.BattleSceneData.SetPlayerCharSprite(battleSprite);

            }
            inputActions.PlayerDigital.Enable();


        }

        // Update is called once per frame
        void Update()
        {
            HandleUpdate();

        }

        public void HandleUpdate()
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
                Application.Quit();

            if (GameManager.Instance.State == GameState.Paused)
            {
                isMoving = false;
                isRunning = false;

                return;
            }

            movement = inputActions.PlayerDigital.Move.ReadValue<Vector2>();
            isRunning = inputActions.PlayerDigital.Run.ReadValue<float>() != 0;

            if (inputActions.PlayerDigital.Interact.triggered)
                StartCoroutine(InteractWith_CR());

            //if (Keyboard.current.mKey.wasPressedThisFrame)
            //  InputMode();
            #region Legacy
            /*
            if (Input.GetKeyUp(KeyCode.Z))
                isNorm = !isNorm;
            if (doLegacyInput)
            {

                if (!isMoving)
                {


                    //change input sistem
                    //movement.x = Input.GetAxisRaw("Horizontal") ;
                    //movement.y = Input.GetAxisRaw("Vertical") ;

                    if (movement != Vector2.zero)
                    {
                        var targetPos = transform.position;
                        targetPos.x += movement.x;
                        targetPos.y += movement.y;

                        StartCoroutine(Move(targetPos));
                    }

                    // ang = Mathf.Atan2(  movement.y, movement.x);
                }
            }
             */

            #endregion

            if (!isMoving)
            {

                Vector2 adjMove;
                //adjMove = movement.normalized;
                adjMove = movement;
                //Debug.Log(adjMove);

                if (adjMove.y != 0) adjMove.x = 0;

                if (movement != Vector2.zero)
                {
                    animator.SetFloat("Horizontal", adjMove.x);
                    animator.SetFloat("Vertical", adjMove.y);


                    var targetPos = transform.position;
                    targetPos.x += Mathf.Round(adjMove.x);
                    targetPos.y += Mathf.Round(adjMove.y);

                    if (IsWakable(targetPos) && inputActions.PlayerDigital.Move.phase == InputActionPhase.Performed)
                        StartCoroutine(Move_CR(targetPos));
                }

                animator.SetBool("IsMoving", isMoving);
                animator.SetBool("IsRunning", isRunning);
            }
            //InteractWith();

            //UpdateAnimation();
        }


        public IEnumerator Move_CR(Vector3 targetPos)
        {
            isMoving = true;

            /*
            Collider2D tile = Physics2D.OverlapCircle(targetPos, 0.185f, LayersManager.i.PortalLayer);
            if (tile != null && tile.TryGetComponent<Portal>(out Portal p))
                yield return p.OnLook(this);
            */
            //var speed = 0f;

            //UpdateAnimation();
            while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                var speed = (isRunning) ? runSpeed : moveSpeed;
                //var change = (movement != Vector2.zero) ? 1 : -1; 
                //speed =  Mathf.Clamp(speed +(change * maxSpeed) / 50f, 0, maxSpeed);


                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                // animator.SetFloat("Speed", movement.sqrMagnitude);
                yield return null;

            }

            //UpdateAnimation();
            //animator.SetFloat("Speed", movement.normalized.sqrMagnitude);

            transform.position = targetPos;

            OnMoveOverTrigger();
            isMoving = false;
            // if (movement == Vector2.zero)
            //   speed = 0f;



        }

        void OnMoveOverTrigger()
        {
            var triggers = Physics2D.OverlapCircleAll(transform.position + new Vector3(0, .3f), 0.3f, LayersManager.i.TriggerableLayers);
            foreach (var t in triggers)
            {
                if (t.TryGetComponent<ITriggerable>(out var trigger))
                {
                    trigger.OnContact(this);
                    break;
                }
            }
        }





        bool IsWakable(Vector3 position)
        {
            if (Physics2D.OverlapCircle(position, 0.185f, LayersManager.i.Unwalkable | LayersManager.i.Interactable) != null)
                return false;


            Collider2D tile = Physics2D.OverlapCircle(position, 0.185f, LayersManager.i.PortalLayer);
            if (tile != null && tile.TryGetComponent<IPortal>(out var p))
                StartCoroutine(p.OnLook_CR(this));


            return true;
        }

        IEnumerator InteractWith_CR()
        {
            //inputActions.PlayerDigital.Move.Disable();
            Vector2 pos = (Vector2)transform.position + new Vector2(animator.GetFloat("Horizontal"), animator.GetFloat("Vertical"));
            var i = Physics2D.OverlapCircle(pos, .01f, LayersManager.i.Interactable);
            //var i = Physics2D.OverlapCircle(pos, .01f);
            if (GameManager.Instance.State == GameState.freeRoam && i != null && i.TryGetComponent<IInteractable>(out var p))
            {
                yield return (p.Interact_CR(transform));
            }

        }

        public Vector2 GetDirection() => movement;

        public object CaptureState()
        {
            var saveData = new PlayerSaveData()
            {
                position = new float[] { transform.position.x, transform.position.y, transform.position.z },
                direction = new float[] { animator.GetFloat("Horizontal"), animator.GetFloat("Vertical") },
                party = GetComponent<PokemonParty>().GetFullParty().Select(p => p.GetSaveData()).ToList(),

            };

            return saveData;

        }

        public void RestoreState(object state)
        {
            var saveData = (PlayerSaveData)state;

            var pos = saveData.position;
            transform.position = new Vector3(pos[0], pos[1], pos[2]);
            var dir = saveData.direction;
            animator.SetFloat("Horizontal", dir[0]);
            animator.SetFloat("Vertical", dir[1]);

            GetComponent<PokemonParty>().Party = saveData.party.Select(p => new Pokemon(p)).ToList();


        }
    }

    [Serializable]
    public class PlayerSaveData
    {
        public float[] position;
        public float[] direction;
        public List<PokemonSaveData> party;
    }

}