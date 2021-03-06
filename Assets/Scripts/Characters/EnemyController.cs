﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour 
{
	public int mHealthPoints;
	public float mSpeed;
	public bool mIsBoss;

	bool mFacingRight = true;
	bool mColliderTouched = false;
	bool mWalking = true;
	bool mDead = false;
	bool mPauseMovement = false;
	bool mDuringRess = false;

	int mBossHP;
	bool mBossDeadOnce = false;
	bool mBossRevived = false;

	Animator mAnimator;
	Transform mSpriteChild;

	void Awake ()
	{
		Physics2D.IgnoreCollision(GameObject.FindGameObjectWithTag("Fireball").GetComponent<Collider2D>(), GetComponent<Collider2D>());
		mAnimator = GetComponentInChildren<Animator>();
		mSpriteChild = GetComponentInChildren<SpriteRenderer>().transform;

		if (mIsBoss)
			mBossHP = mHealthPoints;
	}

	void Update ()
	{
		if (mFacingRight)
			FaceDirection (new Vector2 (1f, 0f));
		else
			FaceDirection (new Vector2 (-1f, 0f));

		if (!mDead && !mPauseMovement)
			Move ();
		
		mAnimator.SetBool("IsWalking", mWalking);
	}

	void Move()
	{
		transform.Translate (new Vector3 (mSpeed, 0f, 0f) * Time.deltaTime);
	}

	void FaceDirection(Vector2 direction)
	{
		int l = -1;

		if (mColliderTouched) {
			mColliderTouched = false;
			mSpriteChild.localScale = new Vector3(mSpriteChild.localScale.x * l, mSpriteChild.localScale.y, mSpriteChild.localScale.z);
		}
			
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "mage")
        {
            collision.gameObject.GetComponent<WizardController>().Damage();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Patrol Collider")
		{
			mSpeed *= -1f;
			if (mFacingRight) {
				mColliderTouched = true;
				mFacingRight = false;
			} else {
				mColliderTouched = true;
				mFacingRight = true;
			}
				
		}
    }

	void Death ()
	{
		mDead = true;
		if (mIsBoss && mBossRevived)
			mAnimator.SetTrigger ("IsDeadBoss");
		else
			mAnimator.SetTrigger ("IsDead");

		if (mIsBoss & !mBossDeadOnce)
			mBossDeadOnce = true;

		if (mIsBoss && mBossDeadOnce && !mBossRevived)
		{
			mBossRevived = true;
			StartCoroutine("Revive");
		}
		else
		{
			if (mIsBoss && mBossRevived)
				GameObject.Find ("Final Door").GetComponent<FinalDoor> ().UnlockDoor ();
			
			Physics2D.IgnoreCollision(GameObject.FindGameObjectWithTag("mage").GetComponent<Collider2D>(), GetComponent<Collider2D>());
			Physics2D.IgnoreCollision(GameObject.FindGameObjectWithTag("Projectile").GetComponent<Collider2D>(), GetComponent<Collider2D>());
			Destroy (gameObject, 2f);
		}

		if (SceneManager.GetActiveScene ().buildIndex == 3 && gameObject.name == "Knight_2")
		{
			GameObject.Find ("Door").GetComponent<Door> ().Open (Vector3.up);
		}
	}

	public void DecrementHP()
	{
		if (mIsBoss && mDuringRess)
			return;
		
		mHealthPoints--;
		GetComponent<AudioSource> ().Play ();

		if (mHealthPoints <= 0)
			Death ();
		else
		{
			mAnimator.SetTrigger ("IsHurt");
			StartCoroutine ("PauseMovement");
		}
	}

	IEnumerator PauseMovement()
	{
		mPauseMovement = true;
		yield return new WaitForSeconds (1f);
		mPauseMovement = false;
	}

	IEnumerator Revive()
	{
		AudioSource[] audioSources;
		audioSources = GameObject.Find ("World").GetComponents<AudioSource> ();

		GameObject.Find ("DialogueText").GetComponent<Text> ().text = "Merlin ! On approche bientôt de la fin… à nous la liberté !";
		audioSources[0].Stop();
		mDuringRess = true;
		yield return new WaitForSeconds (3f);
		mAnimator.SetTrigger ("IsRevived");
		GameObject.Find ("DialogueText").GetComponent<Text> ().text = "Hoho… On dirait que j’ai parlé trop vite… Il nous reste un dernier combat apparemment !";
		audioSources[1].Play();
		mHealthPoints = mBossHP;
		yield return new WaitForSeconds (3f);
		GameObject.Find ("DialogueText").GetComponent<Text> ().text = "";
		mDuringRess = false;
		mDead = false;
	}
}
