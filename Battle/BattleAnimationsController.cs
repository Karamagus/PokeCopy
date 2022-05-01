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

        public IEnumerator AnimateMoveUsage_CR(Unit user, Unit target = null, Move move = null)
        {

            user.animator.Play("Battle_EnemyAttack");

            yield return new WaitForSeconds(user.animator.GetCurrentAnimatorStateInfo(0).length);

            yield return PlayImpact_CR(target, move);
            /*
            if (move.Base.Category != MoveCategory.Status)
                target.impact.Play();

            */

            //set animation event for ipact on target
            //wait while playing use move animation
            //play impact animation on event and wait till end


            yield return null;
        }


        public IEnumerator EffectAnimation_CR(Unit affected)
        {
            //change
            affected.animator.Play("Battle_EnemyStatChange");
            yield return new WaitForSeconds(affected.animator.GetCurrentAnimatorStateInfo(0).length);
            yield return null;
        }


        public IEnumerator FaintAnimation_CR(Unit fainted)
        {
            fainted.animator.Play("Battle_EnemyFainted");
            yield return new WaitForSeconds(fainted.animator.GetCurrentAnimatorStateInfo(0).length);

            //wait until aniamtion ends with clipinfo
            yield return null;
        }

        public IEnumerator SwitchPokemon_CR(Unit Switched)
        {
            Switched.animator.Play("BattlePlayer_Recall");
            yield return new WaitForSeconds(Switched.animator.GetCurrentAnimatorStateInfo(0).length);


            yield return null;
        }
        public IEnumerator PlayImpact_CR(object targetO, Move move)
        {
            var target = (Unit)targetO;
            target.animator.Play("BattlePlayer_Impact");
            if (move.Base.Category != MoveCategory.Status)
                target.impact.Play();

            yield return new WaitForSeconds(target.animator.GetCurrentAnimatorStateInfo(0).length);

        }


        public IEnumerator PlayThrowAnimation_CR(Animator ballAni, Unit target)
        {
            //ballAni.Play("PokeBall_Throw");
            //yield return new WaitForSeconds(ballAni.GetCurrentAnimatorStateInfo(0).length);
            ballAni.Play("PokeBall_Catch1");
            target.animator.Play("BattleEnemy_Catching");
            yield return new WaitForSeconds(ballAni.GetCurrentAnimatorStateInfo(0).length);
        }

        public IEnumerator PlayShakeAnimation_CR(Animator ball)
        {
            yield return null;
            ball.Play("PokeBall_Shake");
            yield return new WaitForSeconds(ball.GetCurrentAnimatorStateInfo(0).length);
        }


        public IEnumerator PlayCatchSuccess_CR(Animator ball)
        {
            yield return null;
            ball.Play("PokeBall_Success");
            yield return new WaitForSeconds(ball.GetCurrentAnimatorStateInfo(0).length);

        }

        public IEnumerator PlayCatchFail_CR(Animator ball, Unit target)
        {
            yield return null;
            ball.Play("PokeBall_Fails");
            yield return new WaitForSeconds(.25f);
            target.animator.Play("BattlePlayer_OutOfPokeBall");
            yield return new WaitForSeconds(ball.GetCurrentAnimatorStateInfo(0).length);


        }

    }

}