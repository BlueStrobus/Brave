using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{  // [SerializeField] + private - ��ȣ����(�� ���Ͽ����� ����)�� �����Ǹ鼭 Inspectorâ���� �����
    // ĳ���� _______________________________________
    [SerializeField]
    private float walkSpeed; // �ȱ� �ӵ�
    [SerializeField]
    private float runSpeed; // �޸��� �ӵ�
    [SerializeField] // �ɱ�
    private float crouchSpeed;
    private float applySpeed; // �ӵ� ����
    // ���� ����
    private bool isRun = false;
    private bool isCrouch = false;

    // �󸶳� ���� ��?
    [SerializeField]
    private float crouchPosY;// ���̴� ����    
    private float originPosY;// ���ƿ��� ����
    private float applyCrouchPosY;

    [SerializeField] // ����
    private float jumpForce; // ���� ��... ���̶� �ٸ��ɱ�?
    private bool isGround = true; // ���� �پ� �ִ°�?; ���߶ٱ� ����
    private CapsuleCollider capsuleCollider; // �� ���� ���� Ȯ��
    //////////////////////////////////////////////////


    // ī�޶�_________________________________________________________
    [SerializeField]
    private float lookSensitivity; // ī�޶� �ΰ���; õõ�� �����̰�
    [SerializeField]
    private float cameraRotationLimit; // ī�޶� ���Ʒ� ������ �Ѱ�(�ѹ��� ��������)
    private float currentCameraRotationX; // ī�޶� ����(��, �Ʒ�)
    [SerializeField]
    private Camera theCamera;
    /////////////////////////////////////////////////////////////////

    private Rigidbody myRigid; // 'myRigid'�̰� �ٸ��� �ص� ��
    // Start�� 'myRigid = GetComponent<Rigidbody>();' �Է�

    // ����!!!!!!!!!!!
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        // 'Component'�߿� 'Rigidbody'�� �����´�
        applySpeed = walkSpeed;

        // �ʱ�ȭ
        originPosY = theCamera.transform.localPosition.y;  // �ɱ�-ī�޶� �Ʒ���
        applyCrouchPosY = originPosY;
    }

    // Update!!!!!!!!!!!!
    void Update()
    {
        IsGround();
        TryJump();
        TryRun(); // �ݵ�� Move();���� �־�� ��
        TryCrouch();
        Move();
        CameraRotation(); // ī�޶� �����̵�(����)
        CharacterRotation(); // ĳ���� �¿� �̵�

    }

    // �ɱ� �õ�
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    // �ɱ� ����
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

    // �ε巯�� �ɱ� ���� ����
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            //_posY = Mathf.Lerp(1,2,0.5f); // �����Լ� (����, ����, ��ȭ����(0���ȵ�))
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
                break;
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }

    // ���� üũ
    private void IsGround()
    { // ���� ��Ҵ��� Ȯ��
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        // y�� ������?��?  +  ���� ����(��������, ��� ��� ����)
    }

    // ���� �õ�
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    // ����
    private void Jump()
    {
        if (isCrouch)
            Crouch();
        myRigid.velocity = transform.up * jumpForce;
    }

    // �޸��� �õ�
    private void TryRun()
    {
        // GetKey�� ������ ���¸� ����
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    // �޸��� ����
    private void Running()
    {
        if (isCrouch)
            Crouch();
        isRun = true;
        applySpeed = runSpeed;
    }

    // �޸��� ���
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    // ������ ����
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal"); // �¿�
        // �Ǽ� '_moveDirX' = Axis �Ʒ�(?) "Horizontal"�� ������
        // "Horizontal" - ���� > ������Ʈ���� > Input Manager > Axes ��ġ��
        float _moveDirZ = Input.GetAxisRaw("Vertical"); // �յ�

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        // normalized - ���� 1�� �������� ����ȭ ��Ű�� 1�ʿ� �󸶳� �̵���ų�� ����ϱ� ������

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
        // '* Time.deltaTime' - 1�ʿ� �̵��ϴ� �����Ӹ�ŭ ������ 1�ʿ� �ѹ�(���⼭ ������� �̵�) �����ϵ��� ��; ����̵� ����
    }

    // ī�޶� ȸ��(���Ʒ���)
    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");// ���콺�� X,Y�� ����
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); // ��,�ּ�, �ִ�

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    // �¿� ĳ���� ȸ��
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }
}