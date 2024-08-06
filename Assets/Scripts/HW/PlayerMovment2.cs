using UnityEngine;

public class PlayerMovment2 : MonoBehaviour
{
   [SerializeField] float moveSpeed = 2f;
   [SerializeField] float runSpeed = 5f;

    void MovePlayer()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movment = new Vector2(horizontal, vertical).normalized * moveSpeed * Time.deltaTime;

        transform.position += movment;

    }

    private void Update()
    {
        MovePlayer();
    }
}
