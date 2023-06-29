using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{  // [SerializeField] + private - 보호수준(이 파일에서만 관여)은 유지되면서 Inspector창에서 제어가능
    // 캐릭터 _______________________________________
    [SerializeField]
    private float walkSpeed; // 걷기 속도
    [SerializeField]
    private float runSpeed; // 달리기 속도
    [SerializeField] // 앉기
    private float crouchSpeed;
    private float applySpeed; // 속도 적용
    // 상태 변수
    private bool isRun = false;
    private bool isCrouch = false;

    // 얼마나 앉을 까?
    [SerializeField]
    private float crouchPosY;// 숙이는 높이    
    private float originPosY;// 돌아오는 높이
    private float applyCrouchPosY;

    [SerializeField] // 점프
    private float jumpForce; // 점프 힘... 높이랑 다른걸까?
    private bool isGround = true; // 땅에 붙어 있는가?; 이중뛰기 막음
    private CapsuleCollider capsuleCollider; // 땅 착지 여부 확인
    //////////////////////////////////////////////////


    // 카메라_________________________________________________________
    [SerializeField]
    private float lookSensitivity; // 카메라 민감도; 천천히 움직이게
    [SerializeField]
    private float cameraRotationLimit; // 카메라 위아레 움직임 한계(한바퀴 못돌리게)
    private float currentCameraRotationX; // 카메라 각도(위, 아래)
    [SerializeField]
    private Camera theCamera;
    /////////////////////////////////////////////////////////////////

    private Rigidbody myRigid; // 'myRigid'이건 다르게 해도 됨
    // Start에 'myRigid = GetComponent<Rigidbody>();' 입력

    // 시작!!!!!!!!!!!
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        // 'Component'중에 'Rigidbody'를 가져온다
        applySpeed = walkSpeed;

        // 초기화
        originPosY = theCamera.transform.localPosition.y;  // 앉기-카메라만 아래로
        applyCrouchPosY = originPosY;
    }

    // Update!!!!!!!!!!!!
    void Update()
    {
        IsGround();
        TryJump();
        TryRun(); // 반드시 Move();위에 있어야 함
        TryCrouch();
        Move();
        CameraRotation(); // 카메라 상하이동(각도)
        CharacterRotation(); // 캐릭터 좌우 이동

    }

    // 앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    // 앉기 동작
    private void Crouch()
    {
        isCrouch = !isCrouch;

        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutine());
    }

    // 부드러운 앉기 동작 실행
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            //_posY = Mathf.Lerp(1,2,0.5f); // 보간함수 (시작, 도착, 변화비율(0은안됨))
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
                break;
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }

    // 지면 체크
    private void IsGround()
    { // 땅에 닿았는지 확인
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        // y값 절반이?뭘?  +  여우 숫자(오르막길, 계단 등들 위해)
    }

    // 점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    // 점프
    private void Jump()
    {
        if (isCrouch)
            Crouch();
        myRigid.velocity = transform.up * jumpForce;
    }

    // 달리기 시도
    private void TryRun()
    {
        // GetKey는 눌러진 상태를 말함
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    // 달리기 실행
    private void Running()
    {
        if (isCrouch)
            Crouch();
        isRun = true;
        applySpeed = runSpeed;
    }

    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    // 움직임 실행
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal"); // 좌우
        // 실수 '_moveDirX' = Axis 아래(?) "Horizontal"을 가져옴
        // "Horizontal" - 에딧 > 프로젝트세팅 > Input Manager > Axes 펼치기
        float _moveDirZ = Input.GetAxisRaw("Vertical"); // 앞뒤

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        // normalized - 합이 1이 나오도록 정규화 시키면 1초에 얼마나 이동시킬지 계산하기 편해짐

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
        // '* Time.deltaTime' - 1초에 이동하는 프레임만큼 나눠서 1초에 한번(여기서 저기까지 이동) 동작하도록 함; 고속이동 방지
    }

    // 카메라 회전(위아래만)
    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");// 마우스는 X,Y만 있음
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); // 값,최소, 최대

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    // 좌우 캐릭터 회전
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }
}