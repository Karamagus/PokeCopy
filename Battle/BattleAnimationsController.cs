using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace pokeCopy
{
    public class BattleAnimationsController : MonoBehaviour
    {

        [SerializeField] AnimatorOverrideController trainerOverride;
        [SerializeField] AnimatorOverrideController wildOverride;

        public AnimatorOverrideController TrainerOverride { get => trainerOverride; }
        public AnimatorOverrideController WildOverride { get => wildOverride; }

        //make a list for animatorOverride of all battle entities

        public void SetMoveAnimation(Move move, Unit user, Unit target = null)
        {
            var usrAnimator = user.animator.runtimeAnimatorController;
            //override attack animation on animator of user

            var moveAni = (user.IsPlayerOrAlly) ? move.Base.UserAnimation : move.Base.OpntAnimation;
            //override user attack animaton
            user.aniControl["Battle_EnemyAttack"] = moveAni;
            //override target impact animation

        }

        //need to pass the state to change
        public void ChangeAnimationState()
        {

        }

        public IEnumerator AnimateMoveUsage(Unit user, Unit target = null, Move move = null)
        {

            user.animator.Play("Battle_EnemyAttack");

            yield return new WaitForSeconds(user.animator.GetCurrentAnimatorStateInfo(0).length);

            yield return PlayImpact(target, move);
            /*
            if (move.Base.Category != MoveCategory.Status)
                target.impact.Play();

            */

            //set animation event for ipact on target
            //wait while playing use move animation
            //play impact animation on event and wait till end


            yield return null;
        }


        public IEnumerator EffectAnimation(Unit affected)
        {
            //change
            affected.animator.Play("Battle_EnemyStatChange");
            yield return new WaitForSeconds(affected.animator.GetCurrentAnimatorStateInfo(0).length);
            yield return null;
        }


        public IEnumerator FaintAnimation(Unit fainted)
        {
            fainted.animator.Play("Battle_EnemyFainted");
            yield return new WaitForSeconds(fainted.animator.GetCurrentAnimatorStateInfo(0).length);

            //wait until aniamtion ends with clipinfo
            yield return null;
        }

        public IEnumerator SwitchPokemon(Unit Switched)
        {
            Switched.animator.Play("BattlePlayer_Recall");
            yield return new WaitForSeconds(Switched.animator.GetCurrentAnimatorStateInfo(0).length);


            yield return null;
        }
        public IEnumerator PlayImpact(object targetO, Move move)
        {
            var target = (Unit)targetO;
            target.animator.Play("BattlePlayer_Impact");
            if (move.Base.Category != MoveCategory.Status)
                target.impact.Play();

            yield return new WaitForSeconds(target.animator.GetCurrentAnimatorStateInfo(0).length);

        }


        public IEnumerator PlayThrowAnimation(Animator ballAni, Unit target)
        {
            //ballAni.Play("PokeBall_Throw");
            //yield return new WaitForSeconds(ballAni.GetCurrentAnimatorStateInfo(0).length);
            ballAni.Play("PokeBall_Catch1");
            target.animator.Play("BattleEnemy_Catching");
            yield return new WaitForSeconds(ballAni.GetCurrentAnimatorStateInfo(0).length);
        }

        public IEnumerator PlayShakeAnimation(Animator ball)
        {
            yield return null;
            ball.Play("PokeBall_Shake");
            yield return new WaitForSeconds(ball.GetCurrentAnimatorStateInfo(0).length);
        }


        public IEnumerator PlayCatchSuccess(Animator ball)
        {
            yield return null;
            ball.Play("PokeBall_Success");
            yield return new WaitForSeconds(ball.GetCurrentAnimatorStateInfo(0).length);

        }

        public IEnumerator PlayCatchFail(Animator ball, Unit target)
        {
            yield return null;
            ball.Play("PokeBall_Fails");
            yield return new WaitForSeconds(.25f);
            target.animator.Play("BattlePlayer_OutOfPokeBall");
            yield return new WaitForSeconds(ball.GetCurrentAnimatorStateInfo(0).length);


        }

    }

}