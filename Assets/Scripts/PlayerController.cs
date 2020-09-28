using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float groundAcceleration = 1;
    [SerializeField] float airAcceleration = 0;
    [SerializeField] float jumpSpeed = 8;
    [SerializeField] float maxSlopeAngle = 45; //max angle of ground you can stand on
    [SerializeField] float gravityStrength = 24;
    [SerializeField] float frictionStrength = 4;

    Rigidbody2D rbody;
    bool isGrounded;
    bool wasGrounded;
    bool wishJump;
    Vector2 playerVelocity;
    Vector2 groundNormal;

    private void Start() {
        rbody = GetComponent<Rigidbody2D>();
    }

    void Update() {
        QueueJump();
    }

    void QueueJump() {
        if (Input.GetButtonDown("Jump")) wishJump = true;
        if (Input.GetButtonUp("Jump")) wishJump = false;
    }

    private void FixedUpdate() {
        if (isGrounded) GroundMove();
        else AirMove();
        rbody.velocity = playerVelocity;

        wasGrounded = isGrounded;
        isGrounded = false;
        groundNormal = Vector2.up;

        Debug.DrawRay(transform.position, playerVelocity, Color.red);
    }

    private void GroundMove() {
        if (wishJump) {
            wishJump = false;
            playerVelocity.y = jumpSpeed;
            return;
        }
        if (!wasGrounded) playerVelocity.y = 0;

        ApplyFriction(frictionStrength);
        Accelerate(groundAcceleration);

        playerVelocity -= Vector2.Dot(playerVelocity, groundNormal) * groundNormal;
    }
    
    private void AirMove() {
        playerVelocity.y -= gravityStrength * Time.fixedDeltaTime;

        Accelerate(airAcceleration);

        if (groundNormal != Vector2.up)
            playerVelocity -= Vector2.Dot(playerVelocity, groundNormal) * groundNormal;
    }

    private void ApplyFriction(float friction) {
        float reduction = friction * Time.fixedDeltaTime;
        if (Mathf.Abs(playerVelocity.x) < reduction) {
            playerVelocity.x = 0; 
            return;
        }

        if (playerVelocity.x > 0) 
            playerVelocity.x -= reduction;
        else 
            playerVelocity.x += reduction;
    }

    private void Accelerate(float acceleration) {
        float currentSpeed = isGrounded ? playerVelocity.magnitude : playerVelocity.x;

        if (currentSpeed < moveSpeed) {
            int wishDir = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
            playerVelocity.x += wishDir * moveSpeed * acceleration * Time.fixedDeltaTime;
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        for (int i = 0; i < collision.contactCount; i++) {
            int velAngle = Mathf.RoundToInt(Vector2.Angle(playerVelocity, collision.contacts[i].normal));
            if (velAngle >= 90 || playerVelocity == Vector2.zero) {
                int angle = Mathf.RoundToInt(Vector2.Angle(Vector2.up, collision.contacts[i].normal));
                if (angle < 90) {
                    groundNormal = collision.contacts[i].normal;
                    if (angle <= maxSlopeAngle)
                        isGrounded = true;
                } else { 
                    playerVelocity -= Vector2.Dot(playerVelocity, collision.contacts[i].normal) * collision.contacts[i].normal;
                }
            }
        }
    }
}
