using UnityEngine;

public class Enemy : Entity
{
    protected override void Start()
    {
        base.Start();
        InvokeRepeating(nameof(TestJump), 4f, 4f);
    }

    /// <summary>
    /// This method is used to test the jump functionality of the enemy. 
    /// This allows the enemy to perform a jump action at regular intervals.
    /// </summary>
    private void TestJump()
    {
        isJumping = true;
        currentJumpTime = jumpTime;
    }
}
