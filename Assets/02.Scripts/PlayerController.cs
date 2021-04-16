using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ���ǵ� ���� ����
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    [SerializeField]
    private float jumpForce;


    // ���� ����
    private bool isWalk = false;
    private bool isRun = false;
    private bool isGround = true;
    private bool isCrouch = false;

    // ������ üũ ����
    private Vector3 lastPos; // �� �������� ���� ��ġ�� ���

    // �ɾ��� �� �󸶳� ������ �����ϴ� ����
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    // �� ���� ����
    private CapsuleCollider capsuleCollider; // �÷��̾�(ĸ�� �ݶ��̴�)�� �ٴڿ� �ô�������� ������ �ǰԲ��Ϸ��� ���� ����

    // �ΰ���
    [SerializeField]
    private float lookSensitivity;

    // ī�޶� �Ѱ�
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0f;

    // �ʿ��� ������Ʈ
    [SerializeField]
    private Camera theCamera;
    // ������ٵ�� ������Ʈ�� �������� ������ ������ �Ѵ�.
    private Rigidbody myRigid;
    // private GunController theGunController;
    // private Crosshair theCrosshair;

    void Start()
    {
        myRigid = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
      //  theGunController = FindObjectOfType<GunController>();
      //  theCrosshair = FindObjectOfType<Crosshair>();

        //�ʱ�ȭ
        originPosY = theCamera.transform.localPosition.y; // localPostition �� ������� ����
        applyCrouchPosY = originPosY; //ó���� �������ϱ�
        applySpeed = walkSpeed;
    }

    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();

    }

    // �ɱ� �õ� 
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    //�ɱ� ����
    private void Crouch()
    {
        isCrouch = !isCrouch;
        isWalk = false;
    //    theCrosshair.WalkingAnimation(isWalk);
    //    theCrosshair.CrouchingAnimation(isCrouch);

        if (isCrouch)
        {
            // �ɱ�
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            // ����
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutine()); //�ڷ�ƾ�� �̿��Ͽ� �ε巯�� �ɱ����
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
    }

    IEnumerator CrouchCoroutine()
    {
        // �ڷ�ƾ : ����ó���� ���ؼ� ������� ����, 
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f); // ����
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
            {
                break;
            }
            yield return null;
        }

        theCamera.transform.localPosition = new Vector3(0f, applyCrouchPosY, 0f);
    }

    // ���� üũ
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }


    // ���� �õ�
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround == true)
        {
            Jump();
        }
    }


    // ����
    private void Jump()
    {
        // velocity�� ���� �����̴� �ӵ�
        // ���������� velocity�� �ٲ㼭 ���߿� ���� �ڵ�

        // ���� ���¿��� ���� �� ���� ���� ����
        if (isCrouch)
            Crouch();


        myRigid.velocity = transform.up * jumpForce;
    }


    // �޸��� �õ�
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift)) // ����Ʈ Ű�� ������ ����
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift)) // ����ƮŰ�� ������ ����
        {
            RunningCancel();
        }
    }


    // �޸��� ����
    private void Running()
    {
        if (isCrouch)
            Crouch();

    //    theGunController.CancelFineSight(); // ������ ���¿��� �޸��� �޸��� Ǯ��

        isRun = true;
    //    theCrosshair.RunningAnimation(isRun);
        applySpeed = runSpeed;
    }


    // �޸��� ���
    private void RunningCancel()
    {
        isRun = false;
    //    theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }


    // ������ ����
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal"); // A D
        float _moveDirZ = Input.GetAxisRaw("Vertical"); // W S

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        // transform.right = (1, 0, 0)

        Vector3 _moveVertical = transform.forward * _moveDirZ;
        // transform.forward = (0, 0, 1)

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        // normallized : ���� 1�� �������� ����ȭ
        // _moveHorizontal + _moveVertical = (1, 0, 1) = 2 �ε�
        // ����ȭ�ϸ� (0.5, 0, 0.5) = 1

        MoveCheck(_velocity);
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime); // �̵���Ŵ
    }

    private void MoveCheck(Vector3 velocity)
    {
        if (!isRun && !isCrouch)
        {
            // lastpos : �� ������ ��ġ , transform.position : ���� ��ġ
            if (velocity.magnitude >= 0.01f)
            {
                isWalk = true;
            }
            else if (velocity.magnitude <= 0.01f)
            {
                isWalk = false;
            }

     //       theCrosshair.WalkingAnimation(isWalk);
            lastPos = transform.position;
        }
    }

    // �¿� ĳ���� ȸ��
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X"); //�÷��̾��� y���� ȸ����Ű�� �¿�� ������
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
        // ����Ƽ���� Rotation�� QuaternionŸ��. 
        // Quaternion.Euler(_characterRotationY) �ڵ�� ������ ���� ���Ͱ��� Quaterninon Ÿ������ �ٲ��ִ°���

    }


    // ���� ī�޶� ȸ��
    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y"); // �� �Ʒ��� ���� ��°�, x�� ȸ����Ű�� ������ ī�޶� �����̼� ���Ѻ��� x���� �����̸� ���Ʒ��� ����Ÿ��°��� ���� �� �� �ִ�.
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX; // ���콺 ������ ���������� += �ϸ� ���οø��� �Ʒ���, �Ʒ��� �ø��� ����, -= �� �� �ݴ��
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); // Mathf.Clamp�� �̿��ؼ� ����, currentCameraRotationX ���� -cameraRotationLimit(-45��)�� cameraRotationLimit(45��) ���̿� ����

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f); // ī�޶� ������ ��ȯ����
    }
}
