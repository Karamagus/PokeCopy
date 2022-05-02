using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{

    public class CharMovement : MonoBehaviour
    {
        public Animator animator;
        [SerializeField] float moveSpeed = 5f;
        float offset = -0.3f;


        public bool IsMoving { get; private set; }

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            SnapPositionToGrid(transform.position);
        }

        private void SetIsMoving(bool value)
        {
            IsMoving = value;
            AnimatorUpdate();

        }

        public void SnapPositionToGrid(Vector3 position)
        {
            position.x = Mathf.Floor(position.x) + .5f;
            position.y = Mathf.Floor(position.y) + .5f + offset;
            transform.position = position;
        }

        public IEnumerator Move(Vector2 moveVec, System.Action OnMoveOver = null)
        {
            if (moveVec == Vector2.zero)
                yield break;

            animator.SetFloat("Horizontal", Mathf.Clamp(moveVec.x, -1, 1));
            animator.SetFloat("Vertical", Mathf.Clamp(moveVec.y, -1, 1));


            var targetPos = transform.position;
            targetPos.x += Mathf.Round(moveVec.x);
            targetPos.y += Mathf.Round(moveVec.y);

            //if (!IsWakable(targetPos))
            if (!IsPathClear(targetPos))
                yield break;

            SetIsMoving(true);

            //UpdateAnimation();
            while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {

                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                // animator.SetFloat("Speed", movement.sqrMagnitude);
                yield return null;

            }

            //UpdateAnimation();
            //animator.SetFloat("Speed", movement.normalized.sqrMagnitude);

            transform.position = targetPos;

            SetIsMoving(false);

            OnMoveOver?.Invoke();


        }

        public void AnimatorUpdate()
        {
            animator.SetBool("IsMoving", IsMoving);

        }


        bool IsPathClear(Vector3 position)
        {
            var diff = position - transform.position;
            var dir = diff.normalized;

            if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, LayersManager.i.Unwalkable | LayersManager.i.Interactable | LayersManager.i.PlayerLayer) == true)
                return false;

            return true;
        }


        bool IsWakable(Vector3 position)
        {
            //if (Physics2D.OverlapCircle(position, 0.3f, LayersManager.i.Unwalkable | LayersManager.i.Interactable | LayersManager.i.PlayerLayer) != null)
            if (Physics2D.Linecast((Vector2)transform.position + new Vector2(animator.GetFloat("Horizontal"), animator.GetFloat("Vertical")), position, LayersManager.i.Unwalkable | LayersManager.i.Interactable | LayersManager.i.PlayerLayer))
                return false;


            return true;
        }



        public void LookTowrds(Vector3 direction)
        {
            var xdif = Mathf.Floor(direction.x) - Mathf.Floor(transform.position.x);
            var ydif = Mathf.Floor(direction.y) - Mathf.Floor(transform.position.y);

            if (xdif == 0 || ydif == 0)
            {
                animator.SetFloat("Horizontal", Mathf.Clamp(xdif, -1, 1));
                animator.SetFloat("Vertical", Mathf.Clamp(ydif, -1, 1));

            }
            else
                Debug.LogError("Error in LookTowrds: Character can't look diagonaly.");



        }


    }
}
