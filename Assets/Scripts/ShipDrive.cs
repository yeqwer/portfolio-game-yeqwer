using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;



public class ShipDrive : MonoBehaviour
{
    public float maxSpeed = 8f;
    public float rotationSpeed = 2f;
    public float speedUpRotation = -7f;
    public float afterMoveVelocity = 5f;
    [HideInInspector] public float maxSpeedStart;

    protected Rigidbody rb;
    public NewControls controls;

    private ParticleSystem partSys;

    private bool flag = false;
    private bool canAfterTimer = true;

    private float timer;

    private CheckLocationForScene1 checkLoc;
    private SoundController soundController;

    public void Awake()
    {
        soundController = FindObjectOfType<SoundController>();
        partSys = GetComponentInChildren<ParticleSystem>();
        rb = GetComponent<Rigidbody>();
        controls = new NewControls();
        checkLoc = GetComponent<CheckLocationForScene1>();
    }
    

    private void Update()
    {
        Timer();

        if (checkLoc.itWater)
        {
            Movement();
        } 
        else 
        { 
            Dropper(2f); 
        }
    }

    private void Movement()
    {
        if (controls.Movement.Joistisk.inProgress)
        {
            Vector2 move = controls.Movement.Joistisk.ReadValue<Vector2>();
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            Vector3 direction = move.x * Camera.main.transform.right + move.y * cameraForward;

            if (direction.magnitude > 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            if (controls.Movement.Shift.inProgress)
            {
                transform.Translate(transform.forward * Time.deltaTime * maxSpeed * 1.5f, Space.World);
                soundController.SoundDrive(1.8f);
            }
            else 
            { 
                transform.Translate(transform.forward * Time.deltaTime * maxSpeed, Space.World);
                soundController.SoundDrive(1.4f);
            } 
            //transform.eulerAngles = new Vector3(speedUpRotation, transform.eulerAngles.y, transform.eulerAngles.z);
            flag = true;

            partSys.Play();
        }
        else
        {
            if (canAfterTimer & flag)
            {
                Forcer();
            }
            soundController.SoundDrive(1f);
            partSys.Stop();
        }
    }

    public void Timer()
    {
        if (timer > 0)
        {
            canAfterTimer = false;
            timer -= Time.deltaTime;
        } else { canAfterTimer = true; }  
        
    }

    private void Forcer()
    {
        rb.AddForce(transform.forward * afterMoveVelocity, ForceMode.Impulse);
        timer = 1f;
        flag = false;
    }

    private void Dropper(float power)
    {
        
        controls.Disable();
        rb.AddForce(-transform.forward.normalized * power, ForceMode.Impulse);
        StartCoroutine(StarterDelay(0.5f));
    }

    IEnumerator StarterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.velocity = Vector3.zero;
        controls.Enable();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}