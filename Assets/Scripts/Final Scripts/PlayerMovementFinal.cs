using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementFinal : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private InputActionReference moveAction;  

    public Rigidbody rb;
    private Vector3 movement;
    public GameObject arcGisCamera;
    public float cameraHeight = 30.37f;
    public GameObject bulletPrefab;
    public Vector2 mousePosition;
    public Transform firePoint;


    public float attackSpeed = 1f;
    private float attackTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        arcGisCamera.transform.position = new Vector3(this.transform.position.x, cameraHeight, this.transform.position.z);
        arcGisCamera.transform.rotation = this.transform.rotation;
        rb.freezeRotation = true;
    }

    void Update()
    {
        movement = moveAction.action.ReadValue<Vector3>();
        attackTimer += Time.deltaTime;

        mousePosition = Mouse.current.position.ReadValue();
        if (attackTimer >= attackSpeed)
        {
            FireAtMousePosition();
            attackTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3(movement.x * moveSpeed, 0, movement.z * moveSpeed);
        arcGisCamera.transform.position = new Vector3(this.transform.position.x, cameraHeight, this.transform.position.z);
    }

    void FireAtMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        Vector3 targetDirection;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            targetDirection = (hit.point - firePoint.position).normalized;
        }
        else
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.y));
            targetDirection = (mouseWorldPos - firePoint.position).normalized;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();

    
        bullet.transform.rotation = Quaternion.LookRotation(targetDirection);

        float bulletSpeed = 50f;
        bulletRB.AddForce(targetDirection * bulletSpeed, ForceMode.VelocityChange);

        StartCoroutine(DestroyBulletAfterTime(bullet, 2f));
    }

    IEnumerator DestroyBulletAfterTime(GameObject bullet, float time)
    {
        yield return new WaitForSeconds(time);
        if (bullet != null)
        {
            Destroy(bullet);
        }
    }
}

