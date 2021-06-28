//-----------------------------------------------------------------------------
// File: CPlayer.cpp
//-----------------------------------------------------------------------------

#include "stdafx.h"
#include "Player.h"
#include "Shader.h"

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CPlayer

CPlayer::CPlayer()
{
	m_pCamera = NULL;

	m_xmf3Position = XMFLOAT3(0.0f, 0.0f, 0.0f);
	m_xmf3Right = XMFLOAT3(1.0f, 0.0f, 0.0f);
	m_xmf3Up = XMFLOAT3(0.0f, 1.0f, 0.0f);
	m_xmf3Look = XMFLOAT3(0.0f, 0.0f, 1.0f);

	m_xmf3Velocity = XMFLOAT3(0.0f, 0.0f, 0.0f);
	m_xmf3Gravity = XMFLOAT3(0.0f, 0.0f, 0.0f);
	m_fMaxVelocityXZ = 0.0f;
	m_fMaxVelocityY = 0.0f;
	m_fFriction = 0.0f;

	m_fPitch = 0.0f;
	m_fRoll = 0.0f;
	m_fYaw = 0.0f;

	m_pPlayerUpdatedContext = NULL;
	m_pCameraUpdatedContext = NULL;
}

CPlayer::~CPlayer()
{
	ReleaseShaderVariables();

	if (m_pCamera) delete m_pCamera;
}

void CPlayer::CreateShaderVariables(ID3D12Device* pd3dDevice, ID3D12GraphicsCommandList* pd3dCommandList)
{
	if (m_pCamera) m_pCamera->CreateShaderVariables(pd3dDevice, pd3dCommandList);
}

void CPlayer::UpdateShaderVariables(ID3D12GraphicsCommandList* pd3dCommandList)
{
}

void CPlayer::ReleaseShaderVariables()
{
	if (m_pCamera) m_pCamera->ReleaseShaderVariables();
}

void CPlayer::Move(DWORD dwDirection, float fDistance, bool bUpdateVelocity)
{
	if (dwDirection)
	{
		XMFLOAT3 xmf3Shift = XMFLOAT3(0, 0, 0);
		if (dwDirection & DIR_FORWARD) xmf3Shift = Vector3::Add(xmf3Shift, m_xmf3Look, fDistance);
		if (dwDirection & DIR_BACKWARD) xmf3Shift = Vector3::Add(xmf3Shift, m_xmf3Look, -fDistance);
		if (dwDirection & DIR_RIGHT) xmf3Shift = Vector3::Add(xmf3Shift, m_xmf3Right, fDistance);
		if (dwDirection & DIR_LEFT) xmf3Shift = Vector3::Add(xmf3Shift, m_xmf3Right, -fDistance);
		if (dwDirection & DIR_UP) xmf3Shift = Vector3::Add(xmf3Shift, m_xmf3Up, fDistance);
		if (dwDirection & DIR_DOWN) xmf3Shift = Vector3::Add(xmf3Shift, m_xmf3Up, -fDistance);


		Move(xmf3Shift, false);


	}
}

void CPlayer::Move(const XMFLOAT3& xmf3Shift, bool bUpdateVelocity)
{
	if (bUpdateVelocity)
	{
		m_xmf3Velocity = Vector3::Add(m_xmf3Velocity, xmf3Shift);
	}
	else
	{
		m_xmf3Position = Vector3::Add(m_xmf3Position, xmf3Shift);
		m_pCamera->Move(xmf3Shift);
	}
}

void CPlayer::Rotate(float x, float y, float z)
{
	DWORD nCurrentCameraMode = m_pCamera->GetMode();
	if ((nCurrentCameraMode == FIRST_PERSON_CAMERA) || (nCurrentCameraMode == THIRD_PERSON_CAMERA))
	{
		m_pCamera->Rotate(x, y, z);
		if (x != 0.0f)
		{
			XMMATRIX xmmtxRotate = XMMatrixRotationAxis(XMLoadFloat3(&m_xmf3Right), XMConvertToRadians(x));
			m_xmf3Look = Vector3::TransformNormal(m_xmf3Look, xmmtxRotate);
			m_xmf3Up = Vector3::TransformNormal(m_xmf3Up, xmmtxRotate);
		}
		if (z != 0.0f)
		{
			XMMATRIX xmmtxRotate = XMMatrixRotationAxis(XMLoadFloat3(&m_xmf3Look), XMConvertToRadians(z));
			m_xmf3Up = Vector3::TransformNormal(m_xmf3Up, xmmtxRotate);
			m_xmf3Right = Vector3::TransformNormal(m_xmf3Right, xmmtxRotate);
		}
		if (y != 0.0f)
		{
			XMMATRIX xmmtxRotate = XMMatrixRotationAxis(XMLoadFloat3(&m_xmf3Up), XMConvertToRadians(y));
			m_xmf3Look = Vector3::TransformNormal(m_xmf3Look, xmmtxRotate);
			m_xmf3Right = Vector3::TransformNormal(m_xmf3Right, xmmtxRotate);
		}
	}
	else if (nCurrentCameraMode == SPACESHIP_CAMERA)
	{
		m_pCamera->Rotate(x, y, z);
		if (x != 0.0f)
		{
			XMMATRIX xmmtxRotate = XMMatrixRotationAxis(XMLoadFloat3(&m_xmf3Right), XMConvertToRadians(x));
			m_xmf3Look = Vector3::TransformNormal(m_xmf3Look, xmmtxRotate);
			m_xmf3Up = Vector3::TransformNormal(m_xmf3Up, xmmtxRotate);
		}
		if (y != 0.0f)
		{
			XMMATRIX xmmtxRotate = XMMatrixRotationAxis(XMLoadFloat3(&m_xmf3Up), XMConvertToRadians(y));
			m_xmf3Look = Vector3::TransformNormal(m_xmf3Look, xmmtxRotate);
			m_xmf3Right = Vector3::TransformNormal(m_xmf3Right, xmmtxRotate);
		}
		if (z != 0.0f)
		{
			XMMATRIX xmmtxRotate = XMMatrixRotationAxis(XMLoadFloat3(&m_xmf3Look), XMConvertToRadians(z));
			m_xmf3Up = Vector3::TransformNormal(m_xmf3Up, xmmtxRotate);
			m_xmf3Right = Vector3::TransformNormal(m_xmf3Right, xmmtxRotate);
		}
	}

	m_xmf3Look = Vector3::Normalize(m_xmf3Look);
	m_xmf3Right = Vector3::CrossProduct(m_xmf3Up, m_xmf3Look, true);
	m_xmf3Up = Vector3::CrossProduct(m_xmf3Look, m_xmf3Right, true);
}

void CPlayer::Update(float fTimeElapsed)
{
	XMFLOAT3 dir{ 0.0f,1.0f,0.0f };

	
	float angle = 80.f - Vector3::Angle(dir, Vector3::Normalize(m_xmf3Up));

	if (isnan(angle))
		angle = 90.f;
	XMFLOAT3 xmf3Gravity = m_xmf3Gravity;
	xmf3Gravity = Vector3::ScalarProduct(xmf3Gravity, angle, false);

	//수정
	m_xmf3Velocity = Vector3::Add(m_xmf3Velocity, xmf3Gravity);


	dir = Vector3::Subtract(m_xmf3Up, dir);
	dir.y = 0.f;
	dir = Vector3::ScalarProduct(dir, 100.f);//500

	float fLength = sqrtf(m_xmf3Velocity.x * m_xmf3Velocity.x + m_xmf3Velocity.z * m_xmf3Velocity.z);
	float fMaxVelocityXZ = m_fMaxVelocityXZ;
	if (fLength > m_fMaxVelocityXZ)
	{
		m_xmf3Velocity.x *= (fMaxVelocityXZ / fLength);
		m_xmf3Velocity.z *= (fMaxVelocityXZ / fLength);
	}
	float fMaxVelocityY = m_fMaxVelocityY;
	fLength = sqrtf(m_xmf3Velocity.y * m_xmf3Velocity.y);
	if (fLength > m_fMaxVelocityY) m_xmf3Velocity.y *= (fMaxVelocityY / fLength);

	m_xmf3Velocity = Vector3::Add(m_xmf3Velocity, dir, 1);
	if (isCollision) {

		m_xmf3Velocity = Vector3::ScalarProduct(m_xmf3Velocity, 0.8f, false);
		
		fknockbackTime += fTimeElapsed;
		if (fknockbackTime > 1.0f) {
			isCollision = false;
			m_bActive = true;
		}
		static float t = 0.0f;
		t += fTimeElapsed;
		if (t > 0.1) {
			t = 0.0f;
			if (m_bActive)
				m_bActive = false;
			else
				m_bActive = true;
		}

	}

	XMFLOAT3 xmf3Velocity = Vector3::ScalarProduct(m_xmf3Velocity, fTimeElapsed, false);
	Move(xmf3Velocity, false);

	if (m_xmf3Position.x < 50) {
		//m_pPlayer->SetLookVectorReflact({ 1.f,0.f,0.f });
		m_xmf3Position.x -= xmf3Velocity.x;
		
	}

	if (m_xmf3Position.x > 1790) {
		//m_pPlayer->SetLookVectorReflact({ 1.f,0.f,0.f });
		m_xmf3Position.x -= xmf3Velocity.x;
		
	}
	if (m_xmf3Position.z < 0) {
		//m_pPlayer->SetLookVectorReflact({ 1.f,0.f,0.f });
		m_xmf3Position.z -= xmf3Velocity.z;
		
	}

	if (m_xmf3Position.z > 1790) {
		//m_pPlayer->SetLookVectorReflact({ 1.f,0.f,0.f });
		m_xmf3Position.z -= xmf3Velocity.z;
		
	}


	if (m_xmf3Position.y > 1000) {
		//m_pPlayer->SetLookVectorReflact({ 1.f,0.f,0.f });
		m_xmf3Position.y -= xmf3Velocity.y;

	}

	//
	//dir = Vector3::ScalarProduct(dir, angle);
	//Move(dir, false);

	//XMFLOAT3 up;
	//std::cout << m_xmf3Up.x<<" " << m_xmf3Up.y <<" " << m_xmf3Up.z << std::endl;

	//if (angle < 80.f)
	//{
	//	up = XMFLOAT3{ 0.0f, std::clamp(1.f - Vector3::Length(dir), 0.f, 1.f), 0.0f };
	//	std::cout << Vector3::Length(dir) << std::endl;
	//	std::cout << Vector3::Length(up) << std::endl;
	//	//dir = Vector3::ScalarProduct(dir, 2.f, false);//내가 가야하는 방향을 구하고
	//}
	//else {
	//	up = XMFLOAT3{ 0.0f, 10.0f, 0.0f };
	//}

	if (m_pPlayerUpdatedContext) OnPlayerUpdateCallback(fTimeElapsed);
	DWORD nCurrentCameraMode = m_pCamera->GetMode();
	if (nCurrentCameraMode == THIRD_PERSON_CAMERA) m_pCamera->Update(m_xmf3Position, fTimeElapsed);
	if (m_pCameraUpdatedContext) OnCameraUpdateCallback(fTimeElapsed);
	if (nCurrentCameraMode == THIRD_PERSON_CAMERA) m_pCamera->SetLookAt(m_xmf3Position);
	m_pCamera->RegenerateViewMatrix();

	fLength = Vector3::Length(m_xmf3Velocity);
	float fDeceleration = (m_fFriction * fTimeElapsed);
	if (fDeceleration > fLength) fDeceleration = fLength;
	m_xmf3Velocity = Vector3::Add(m_xmf3Velocity, Vector3::ScalarProduct(m_xmf3Velocity, -fDeceleration, true));
}

CCamera* CPlayer::OnChangeCamera(DWORD nNewCameraMode, DWORD nCurrentCameraMode)
{
	CCamera* pNewCamera = NULL;
	switch (nNewCameraMode)
	{
	case FIRST_PERSON_CAMERA:
		pNewCamera = new CFirstPersonCamera(m_pCamera);
		break;
	case THIRD_PERSON_CAMERA:
		pNewCamera = new CThirdPersonCamera(m_pCamera);
		break;
	case SPACESHIP_CAMERA:
		pNewCamera = new CSpaceShipCamera(m_pCamera);
		break;
	}
	if (nCurrentCameraMode == SPACESHIP_CAMERA)
	{
		m_xmf3Right = Vector3::Normalize(XMFLOAT3(m_xmf3Right.x, 0.0f, m_xmf3Right.z));
		m_xmf3Up = Vector3::Normalize(XMFLOAT3(0.0f, 1.0f, 0.0f));
		m_xmf3Look = Vector3::Normalize(XMFLOAT3(m_xmf3Look.x, 0.0f, m_xmf3Look.z));

		m_fPitch = 0.0f;
		m_fRoll = 0.0f;
		m_fYaw = Vector3::Angle(XMFLOAT3(0.0f, 0.0f, 1.0f), m_xmf3Look);
		if (m_xmf3Look.x < 0.0f) m_fYaw = -m_fYaw;
	}
	else if ((nNewCameraMode == SPACESHIP_CAMERA) && m_pCamera)
	{
		m_xmf3Right = m_pCamera->GetRightVector();
		m_xmf3Up = m_pCamera->GetUpVector();
		m_xmf3Look = m_pCamera->GetLookVector();
	}

	if (pNewCamera)
	{
		pNewCamera->SetMode(nNewCameraMode);
		pNewCamera->SetPlayer(this);
	}

	if (m_pCamera) delete m_pCamera;

	return(pNewCamera);
}

void CPlayer::OnPrepareRender()
{
	m_xmf4x4Transform._11 = m_xmf3Right.x; m_xmf4x4Transform._12 = m_xmf3Right.y; m_xmf4x4Transform._13 = m_xmf3Right.z;
	m_xmf4x4Transform._21 = m_xmf3Up.x; m_xmf4x4Transform._22 = m_xmf3Up.y; m_xmf4x4Transform._23 = m_xmf3Up.z;
	m_xmf4x4Transform._31 = m_xmf3Look.x; m_xmf4x4Transform._32 = m_xmf3Look.y; m_xmf4x4Transform._33 = m_xmf3Look.z;
	m_xmf4x4Transform._41 = m_xmf3Position.x; m_xmf4x4Transform._42 = m_xmf3Position.y; m_xmf4x4Transform._43 = m_xmf3Position.z;

	UpdateTransform(NULL);
}

void CPlayer::Render(ID3D12GraphicsCommandList* pd3dCommandList, CCamera* pCamera)
{
	DWORD nCameraMode = (pCamera) ? pCamera->GetMode() : 0x00;
	if (nCameraMode == THIRD_PERSON_CAMERA) CGameObject::Render(pd3dCommandList, pCamera);
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CAirplanePlayer

CAirplanePlayer::CAirplanePlayer(ID3D12Device* pd3dDevice, ID3D12GraphicsCommandList* pd3dCommandList, ID3D12RootSignature* pd3dGraphicsRootSignature, void* pContext)
{
	m_pCamera = ChangeCamera(/*SPACESHIP_CAMERA*/THIRD_PERSON_CAMERA, 0.0f);

	CHeightMapTerrain* pTerrain = reinterpret_cast<CHeightMapTerrain*>(pContext);
	SetPlayerUpdatedContext(pTerrain);
	SetCameraUpdatedContext(pTerrain);

	//	CGameObject *pGameObject = CGameObject::LoadGeometryFromFile(pd3dDevice, pd3dCommandList, pd3dGraphicsRootSignature, "Model/Apache.bin");
	CGameObject* pGameObject = CGameObject::LoadGeometryFromFile(pd3dDevice, pd3dCommandList, pd3dGraphicsRootSignature, "Model/Apache.bin");
	pGameObject->SetPosition(0.f, 0.f, 0.f);
	pGameObject->Rotate(0.0f, 0.0f, 0.0f);
	pGameObject->SetScale(0.1f, 0.1f, 0.1f);
	SetChild(pGameObject, true);

	OnInitialize();

	m_xmOOBB = BoundingOrientedBox(XMFLOAT3(0.0f, 0.0f, 0.0f), XMFLOAT3(10.0f, 10.0f, 10.0f), XMFLOAT4(0.0f, 0.0f, 0.0f, 1.0f));

	CMissileObject* pMissileObject = NULL;


	m_ppMissiles = new CMissileObject * [nMissiles];
	for (int i = 0; i < nMissiles; ++i)
	{
		CGameObject* pMissileModel = CGameObject::LoadGeometryFromFile(pd3dDevice, pd3dCommandList, pd3dGraphicsRootSignature, "Model/missile.bin");

		pMissileObject = new CMissileObject();
		pMissileObject->SetChild(pMissileModel, true);
		pMissileObject->SetPosition(GetPosition());
		//pMissileObject->SetScale(0.5f, 0.5f, 0.5f);
		pMissileObject->Rotate(0.0f, 0.0f, 0.0f);
		pMissileObject->SetActive(false);
		pMissileObject->SetMovingSpeed(500.f);
		pMissileObject->SetObjectUpdatedContext(pTerrain);
		m_ppMissiles[i] = pMissileObject;
	}

	CreateShaderVariables(pd3dDevice, pd3dCommandList);
}

CAirplanePlayer::~CAirplanePlayer()
{
	//

	if (m_ppMissiles)
	{

		for (int i = 0; i < nMissiles; i++) if (m_ppMissiles[i]) m_ppMissiles[i]->Release();
		delete[] m_ppMissiles;
	}

}

void CAirplanePlayer::OnInitialize()
{
	m_pMainRotorFrame = FindFrame("rotor");
	m_pTailRotorFrame = FindFrame("black_m_7");

}

void CAirplanePlayer::Animate(float fTimeElapsed, XMFLOAT4X4* pxmf4x4Parent)
{
	if (m_pMainRotorFrame)
	{
		XMMATRIX xmmtxRotate = XMMatrixRotationY(XMConvertToRadians(360.0f * 2.0f) * fTimeElapsed);
		m_pMainRotorFrame->m_xmf4x4Transform = Matrix4x4::Multiply(xmmtxRotate, m_pMainRotorFrame->m_xmf4x4Transform);
	}
	if (m_pTailRotorFrame)
	{
		XMMATRIX xmmtxRotate = XMMatrixRotationX(XMConvertToRadians(360.0f * 4.0f) * fTimeElapsed);
		m_pTailRotorFrame->m_xmf4x4Transform = Matrix4x4::Multiply(xmmtxRotate, m_pTailRotorFrame->m_xmf4x4Transform);
	}
	for (int i = 0; i < nMissiles; i++) if (m_ppMissiles[i]->m_bActive) m_ppMissiles[i]->Animate(fTimeElapsed, pxmf4x4Parent);


	CPlayer::Animate(fTimeElapsed, pxmf4x4Parent);
}

void CAirplanePlayer::Render(ID3D12GraphicsCommandList* pd3dCommandList, CCamera* pCamera)
{
	if (m_bActive)
	{
		CPlayer::Render(pd3dCommandList, pCamera);
	}
	for (int i = 0; i < nMissiles; i++) if (m_ppMissiles[i]->m_bActive) m_ppMissiles[i]->Render(pd3dCommandList, pCamera);
}

void CAirplanePlayer::OnPrepareRender()
{
	CPlayer::OnPrepareRender();
}

void CAirplanePlayer::OnPlayerUpdateCallback(float fTimeElapsed)
{
	XMFLOAT3 xmf3PlayerPosition = GetPosition();
	CHeightMapTerrain* pTerrain = (CHeightMapTerrain*)m_pPlayerUpdatedContext;
	/*지형에서 플레이어의 현재 위치 (x, z)의 지형 높이(y)를 구한다. 그리고 플레이어 메쉬의 높이가 12이고 플레이어의
	중심이 직육면체의 가운데이므로 y 값에 메쉬의 높이의 절반을 더하면 플레이어의 위치가 된다.*/

	float fHeight = pTerrain->GetHeight(xmf3PlayerPosition.x, xmf3PlayerPosition.z) + 6.0f;

	/*플레이어의 위치 벡터의 y-값이 음수이면(예를 들어, 중력이 적용되는 경우) 플레이어의 위치 벡터의 y-값이 점점
	작아지게 된다. 이때 플레이어의 현재 위치 벡터의 y 값이 지형의 높이(실제로 지형의 높이 + 6)보다 작으면 플레이어
	의 일부가 지형 아래에 있게 된다. 이러한 경우를 방지하려면 플레이어의 속도 벡터의 y 값을 0으로 만들고 플레이어
	의 위치 벡터의 y-값을 지형의 높이(실제로 지형의 높이 + 6)로 설정한다. 그러면 플레이어는 항상 지형 위에 있게 된
	다.*/

	if (xmf3PlayerPosition.y < fHeight)
	{
		XMFLOAT3 xmf3PlayerVelocity = GetVelocity();
		xmf3PlayerVelocity.y = 0.0f;
		SetVelocity(xmf3PlayerVelocity);
		xmf3PlayerPosition.y = fHeight;
		SetPosition(xmf3PlayerPosition);
	}
}

void CAirplanePlayer::OnCameraUpdateCallback(float fTimeElapsed)
{
	XMFLOAT3 xmf3CameraPosition = m_pCamera->GetPosition();
	/*높이 맵에서 카메라의 현재 위치 (x, z)에 대한 지형의 높이(y 값)를 구한다. 이 값이 카메라의 위치 벡터의 y-값 보
	다 크면 카메라가 지형의 아래에 있게 된다. 이렇게 되면 다음 그림의 왼쪽과 같이 지형이 그려지지 않는 경우가 발생
	한다(카메라가 지형 안에 있으므로 삼각형의 와인딩 순서가 바뀐다). 이러한 경우가 발생하지 않도록 카메라의 위치 벡
	터의 y-값의 최소값은 (지형의 높이 + 5)로 설정한다. 카메라의 위치 벡터의 y-값의 최소값은 지형의 모든 위치에서
	카메라가 지형 아래에 위치하지 않도록 설정해야 한다.*/
	CHeightMapTerrain* pTerrain = (CHeightMapTerrain*)m_pCameraUpdatedContext;
	float fHeight = pTerrain->GetHeight(xmf3CameraPosition.x, xmf3CameraPosition.z) +
		5.0f;
	if (xmf3CameraPosition.y <= fHeight)
	{
		xmf3CameraPosition.y = fHeight;
		m_pCamera->SetPosition(xmf3CameraPosition);
		if (m_pCamera->GetMode() == THIRD_PERSON_CAMERA)
		{
			//3인칭 카메라의 경우 카메라 위치(y-좌표)가 변경되었으므로 카메라가 플레이어를 바라보도록 한다. 
			CThirdPersonCamera* p3rdPersonCamera = (CThirdPersonCamera*)m_pCamera;
			p3rdPersonCamera->SetLookAt(GetPosition());
		}
	}
}

void CAirplanePlayer::FireBullet(CGameObject* pSelectedObject)
{

	CMissileObject* pMissiles = NULL;

	for (int i = 0; i < nMissiles; i++)
	{
		if (!m_ppMissiles[i]->m_bActive)
		{
			pMissiles = m_ppMissiles[i];

			if (pSelectedObject) pMissiles->m_pTarget = pSelectedObject;

			break;
		}
	}

	if (pMissiles)
	{
		pMissiles->m_xmf4x4Transform = Matrix4x4::Identity();
		pMissiles->m_xmf4x4World = Matrix4x4::Identity();
		pMissiles->SetScale(0.1f, 0.1f, 0.1f);
		XMFLOAT3 xmf3Position = GetPosition();
		XMFLOAT3 xmf3Direction = GetLook();

		XMFLOAT3 xmf3X{ 1.0f,0.0f,0.0f };
		XMFLOAT3 xmf3Y{ 0.0f,1.0f,0.0f };
		XMFLOAT3 xmf3Z{ 0.0f,0.0f,1.0f };
		xmf3Direction.y = 0;
		xmf3Direction = Vector3::Normalize(xmf3Direction);

		float Yangle{};
		
		if (xmf3Direction.x > 0.0f)
			Yangle = XMConvertToDegrees(acosf(Vector3::DotProduct(xmf3Z, xmf3Direction)));
		else
			Yangle = 360.f - XMConvertToDegrees(acosf(Vector3::DotProduct(xmf3Z, xmf3Direction))) ;



		xmf3Direction = GetLook();

		pMissiles->Rotate(0.0, Yangle, 0.0);
		//pMissiles->Rotate(Xangle, 0.0, 0.0);
		pMissiles->SetStartPosition(xmf3Position);
		pMissiles->SetMovingDirection(xmf3Direction);
		pMissiles->SetActive(true);
	}
}

CCamera* CAirplanePlayer::ChangeCamera(DWORD nNewCameraMode, float fTimeElapsed)
{
	DWORD nCurrentCameraMode = (m_pCamera) ? m_pCamera->GetMode() : 0x00;
	if (nCurrentCameraMode == nNewCameraMode) return(m_pCamera);
	switch (nNewCameraMode)
	{
	case FIRST_PERSON_CAMERA:
		SetFriction(2.0f);
		SetGravity(XMFLOAT3(0.0f, 0.0f, 0.0f));
		SetMaxVelocityXZ(40.5f);
		SetMaxVelocityY(40.0f);
		m_pCamera = OnChangeCamera(FIRST_PERSON_CAMERA, nCurrentCameraMode);
		m_pCamera->SetTimeLag(0.0f);
		m_pCamera->SetOffset(XMFLOAT3(0.0f, 20.0f, 0.0f));
		m_pCamera->GenerateProjectionMatrix(1.01f, 5000.0f, ASPECT_RATIO, 60.0f);
		m_pCamera->SetViewport(0, 0, FRAME_BUFFER_WIDTH, FRAME_BUFFER_HEIGHT, 0.0f, 1.0f);
		m_pCamera->SetScissorRect(0, 0, FRAME_BUFFER_WIDTH, FRAME_BUFFER_HEIGHT);
		break;
	case SPACESHIP_CAMERA:
		SetFriction(100.5f);
		SetGravity(XMFLOAT3(0.0f, 0.0f, 0.0f));
		SetMaxVelocityXZ(40.0f);
		SetMaxVelocityY(40.0f);
		m_pCamera = OnChangeCamera(SPACESHIP_CAMERA, nCurrentCameraMode);
		m_pCamera->SetTimeLag(0.0f);
		m_pCamera->SetOffset(XMFLOAT3(0.0f, 0.0f, 0.0f));
		m_pCamera->GenerateProjectionMatrix(1.01f, 5000.0f, ASPECT_RATIO, 60.0f);
		m_pCamera->SetViewport(0, 0, FRAME_BUFFER_WIDTH, FRAME_BUFFER_HEIGHT, 0.0f, 1.0f);
		m_pCamera->SetScissorRect(0, 0, FRAME_BUFFER_WIDTH, FRAME_BUFFER_HEIGHT);
		break;
	case THIRD_PERSON_CAMERA:
		SetFriction(20.5f);
		SetGravity(XMFLOAT3(0.0f, -5.0f, 0.0f));
		SetMaxVelocityXZ(20.0f);
		SetMaxVelocityY(50.0f);
		m_pCamera = OnChangeCamera(THIRD_PERSON_CAMERA, nCurrentCameraMode);
		m_pCamera->SetTimeLag(0.25f);
		m_pCamera->SetOffset(XMFLOAT3(0.0f, 5.0f, -30.0f));
		m_pCamera->GenerateProjectionMatrix(1.01f, 5000.0f, ASPECT_RATIO, 60.0f);
		m_pCamera->SetViewport(0, 0, FRAME_BUFFER_WIDTH, FRAME_BUFFER_HEIGHT, 0.0f, 1.0f);
		m_pCamera->SetScissorRect(0, 0, FRAME_BUFFER_WIDTH, FRAME_BUFFER_HEIGHT);
		break;
	default:
		break;
	}

	m_pCamera->SetPosition(Vector3::Add(m_xmf3Position, m_pCamera->GetOffset()));
	Update(fTimeElapsed);

	return(m_pCamera);
}

