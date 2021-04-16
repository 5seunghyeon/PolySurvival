using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    [SerializeField]
    private float jumpForce;


    // 상태 변수
    private bool isWalk = false;
    private bool isRun = false;
    private bool isGround = true;
    private bool isCrouch = false;

    // 움직임 체크 변수
    private Vector3 lastPos; // 전 프레임의 현재 위치를 기억

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    // 땅 착지 여부
    private CapsuleCollider capsuleCollider; // 플레이어(캡슐 콜라이더)가 바닥에 맡닿았을때만 점프가 되게끔하려고 만든 변수

    // 민감도
    [SerializeField]
    private float lookSensitivity;

    // 카메라 한계
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0f;

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;
    // 리지드바디는 오브젝트에 물리적인 성질을 가지게 한다.
    private Rigidbody myRigid;
    // private GunController theGunController;
    // private Crosshair theCrosshair;

    void Start()
    {
        myRigid = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
      //  theGunController = FindObjectOfType<GunController>();
      //  theCrosshair = FindObjectOfType<Crosshair>();

        //초기화
        originPosY = theCamera.transform.localPosition.y; // localPostition 은 상대적인 변수
        applyCrouchPosY = originPosY; //처음엔 서있으니까
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

    // 앉기 시도 
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    //앉기 동작
    private void Crouch()
    {
        isCrouch = !isCrouch;
        isWalk = false;
    //    theCrosshair.WalkingAnimation(isWalk);
    //    theCrosshair.CrouchingAnimation(isCrouch);

        if (isCrouch)
        {
            // 앉기
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            // 서기
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutine()); //코루틴을 이용하여 부드러운 앉기시점
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
    }

    IEnumerator CrouchCoroutine()
    {
        // 코루틴 : 병렬처리를 위해서 만들어진 개념, 
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f); // 보간
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
            {
                break;
            }
            yield return null;
        }

        theCamera.transform.localPosition = new Vector3(0f, applyCrouchPosY, 0f);
    }

    // 지면 체크
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }


    // 점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround == true)
        {
            Jump();
        }
    }


    // 점프
    private void Jump()
    {
        // velocity는 현재 움직이는 속도
        // 순간적으로 velocity를 바꿔서 공중에 띄우는 코드

        // 앉은 상태에서 점프 시 앉은 상태 해제
        if (isCrouch)
            Crouch();


        myRigid.velocity = transform.up * jumpForce;
    }


    // 달리기 시도
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift)) // 쉬프트 키를 누르는 동안
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift)) // 쉬프트키가 떼지는 순간
        {
            RunningCancel();
        }
    }


    // 달리기 실행
    private void Running()
    {
        if (isCrouch)
            Crouch();

    //    theGunController.CancelFineSight(); // 정조준 상태에서 달리면 달리기 풀림

        isRun = true;
    //    theCrosshair.RunningAnimation(isRun);
        applySpeed = runSpeed;
    }


    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
    //    theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }


    // 움직임 실행
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal"); // A D
        float _moveDirZ = Input.GetAxisRaw("Vertical"); // W S

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        // transform.right = (1, 0, 0)

        Vector3 _moveVertical = transform.forward * _moveDirZ;
        // transform.forward = (0, 0, 1)

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        // normallized : 합이 1이 나오도록 정규화
        // _moveHorizontal + _moveVertical = (1, 0, 1) = 2 인데
        // 정규화하면 (0.5, 0, 0.5) = 1

        MoveCheck(_velocity);
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime); // 이동시킴
    }

    private void MoveCheck(Vector3 velocity)
    {
        if (!isRun && !isCrouch)
        {
            // lastpos : 전 프레임 위치 , transform.position : 현재 위치
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

    // 좌우 캐릭터 회전
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X"); //플레이어의 y축을 회전시키면 좌우로 움직임
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
        // 유니티에서 Rotation은 Quaternion타입. 
        // Quaternion.Euler(_characterRotationY) 코드는 위에서 구한 백터값을 Quaterninon 타입으로 바꿔주는거임

    }


    // 상하 카메라 회전
    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y"); // 위 아래로 고개를 드는것, x를 회전시키는 이유는 카메라를 로테이션 시켜보면 x축을 움직이면 위아래로 까딱거리는것을 관찰 할 수 있다.
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX; // 마우스 반전과 관련이있음 += 하면 위로올리면 아래로, 아래로 올리면 위로, -= 는 그 반대로
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); // Mathf.Clamp를 이용해서 고정, currentCameraRotationX 값이 -cameraRotationLimit(-45도)와 cameraRotationLimit(45도) 사이에 고정

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f); // 카메라 각도를 변환해줌
    }
}
