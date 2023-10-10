using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TicTacToe : MonoBehaviour
{
    enum State
    {
        Start = 0,
        Game,
        End
    };

    enum Turn
    {
        I = 0,
        You
    }

    enum Stone
    {
        None = 0,
        StoneX,
        StoneO
    }

    // 통신용 변수 선언
    Tcp tcp;
    public InputField ip;

    // Texture 변수 선언
    public Texture texBoard;
    public Texture texX;
    public Texture texO;

    public Texture turnX;
    public Texture turnO;

    public Texture winnerX;
    public Texture winnerO;
    public Texture draw;

    // 3*3 보드 배열 선언
    int[] board = new int[9];
    // 리스트는 크기가 정해져 있지 않아서 유동적으로 사용 가능
    // 배열로 선언하면 크기를 변경하기 위해 코드를 일일히 수정해야 함

    // 게임 상태 저장할 변수
    State state;

    // 표시할 순서, 내 차례, 상대 차례, 승자
    Stone stoneTurn;
    Stone stoneI;
    Stone stoneYou;
    Stone stoneWinner;

    Button buttonServer;
    InputField ipInputField;
    Button buttonClient;

    Button buttonRestart;

    void Start()
    {
        // 현재 게임 오브젝트에 붙어 있는 'TCP' 컴포넌트의 인스턴스를 tcp변수에 할당
        tcp = GetComponent<Tcp>();
        // 게임이 시작될 때 state 변수를 State.Start로 초기화
        state = State.Start;
        // board 배열의 각 요소를 None으로 초기화
        for (int i = 0; i < board.Length; ++i)
        {
            // enum Stone에서 None에 해당하는 값을 정수로 변환한 것
            board[i] = (int)Stone.None;
        }

        buttonServer = GameObject.Find("ButtonServer").GetComponent<Button>();
        ipInputField = GameObject.Find("InputFieldIP").GetComponent<InputField>();
        buttonClient = GameObject.Find("ButtonClient").GetComponent<Button>();
        buttonRestart = GameObject.Find("ButtonRestart").GetComponent<Button>();
        buttonRestart.gameObject.SetActive(false);
    }

    public void ServerStart()
    {
        tcp.StartServer(10000, 10);

        // 서버가 시작되면 서버, 클라이언트 버튼과 InputField를 비활성화
        if (buttonServer != null)
        {
            buttonServer.gameObject.SetActive(false);
        }
        if (ipInputField != null)
        {
            ipInputField.gameObject.SetActive(false);
        }
        if (buttonClient != null)
        {
            buttonClient.gameObject.SetActive(false);
        }
    }

    public void ClientStart()
    {
        tcp.Connect(ip.text, 10000);

        // 클라이언트가 연결되면 서버, 클라이언트 버튼과 InputField를 비활성화
        if (buttonServer != null)
        {
            buttonServer.gameObject.SetActive(false);
        }
        if (ipInputField != null)
        {
            ipInputField.gameObject.SetActive(false);
        }
        if (buttonClient != null)
        {
            buttonClient.gameObject.SetActive(false);
        }
    }


    //  매 프레임마다 호출 (GUI를 그리기 위해 자동으로 호출되는 메서드)
    void OnGUI()
    {
        // Repaint 이벤트가 발생할 때마다 화면의 지정된 영역의 'texBoard'라는 텍스쳐를 그림
        if(!Event.current.type.Equals(EventType.Repaint)) return;
        // 왼쪽 위 모서리(660,300)에서 시작하여 가로 600픽셀, 세로 600픽셀의 사각형 영역의 텍스쳐를 그림
        Graphics.DrawTexture(new Rect(660, 300, 600, 600), texBoard);


        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] != (int)Stone.None)
            {
                // i를 나눈 나머지 값에 195을 곱함 (각 텍스쳐의 간격)
                // 위치를 조절한 결과를 30만큼 이동시켜서 최종적인 좌표 계산
                float x = 30 + 660 + (i % 3) * 195;
                float y = 30 + 300 + (i / 3) * 195;

                Texture tex = (board[i] == (int)Stone.StoneO) ? texO : texX;
                // 계산된 위치에 150x150 크기의 텍스쳐를 그림
                Graphics.DrawTexture(new Rect(x, y, 150, 150), tex);
            }
        }

        // 게임 진행 상태
        if (state == State.Game)
        {
            if(stoneTurn == Stone.StoneX)
             {
                 Graphics.DrawTexture(new Rect(1320, 400, 300, 100), turnX);
             }
             else
             {
                 Graphics.DrawTexture(new Rect(300, 400, 300, 100), turnO);
             }
        }

        // 게임 종료 상태
        if(state == State.End)
        {
            UpdateEnd();
        }
    }

    void Update()
    {
        if (!tcp.IsConnect()) return;
        
        if(state == State.Start)
        {
            UpdateStart();
        }

        if(state == State.Game)
        {
            UpdateGame();
        }

        if(state == State.End)
        {
            UpdateEnd();
        }
    }

    void UpdateStart()
    {
        // 게임 모드 상태로 변경
        state = State.Game;
        // O 놓을 차례
        stoneTurn = Stone.StoneO;

        // 서버(나) 표시 = O
        if (tcp.IsServer())
        {
            stoneI = Stone.StoneO;
            stoneYou = Stone.StoneX;
        }

        // 클라이언트(상대) 표시 = X
        else
        {
            stoneI = Stone.StoneX;
            stoneYou = Stone.StoneO;
        }

    }

    void UpdateGame()
    {
        bool bSet = false;

        if(stoneTurn == stoneI)
        {
            bSet = MyTurn();
        }
        else
        {
            bSet = YourTurn();
        }
        if (bSet == false)
        {
            return;
        }

        stoneWinner = CheckBoard();

        if(stoneWinner != Stone.None)
        {
            state = State.End;
            Debug.Log("승리: " + (int)stoneWinner);
        }

        else if (CheckDraw()) // 무승부 체크
        {
            state = State.End;
            Debug.Log("무승부");
        }

        stoneTurn = (stoneTurn == Stone.StoneO) ? Stone.StoneX : Stone.StoneO;
    }

    bool CheckDraw()
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == (int)Stone.None)
            {
                return false; // 아직 빈 자리가 있으면 계속 진행
            }
        }
        return true; // 모든 자리가 차면 무승부
    }


    //클라이언트는 서버로부터 받은 정보를 이용하여 상대방이 둔 돌을 자신의 게임 보드에 표시
    bool YourTurn()
    {
        // 크기가 1인 바이트 배열 'data'를 생성, 서버에서 받은 데이터를 저장할 용도
        byte[] data = new byte[1];
        // tcp.Receive 메서드를 호출하여 서버로부터 데이터를 받아옴
        int iSize = tcp.Receive(ref data, data.Length);

        if (iSize <= 0)
        {
            return false;
        }

        // 수신된 데이터 배열의 첫 번째 요소를 정수형으로 변환하여 'i'에 저장
        int i = (int)data[0];
        Debug.Log("받음:" + i);

        // 받은 데이터를 이용하여 상대방이 둔 돌을 게임보드에 배치하는 'SetStone' 메서드를 호출
        bool ret = SetStone(i, stoneYou);
        if(ret == false)
        {
            return false;
        }
        return true;
    }

    Stone CheckBoard()
    {
        // 두가지의 돌 상태(StoneO,StoneX)에 대해 각각 승리 여부를 체크
        for (int i = 0; i < 2; i++)
        {
            int s;
            if (i == 0)
            {
                s = (int)Stone.StoneO; // 2
            }
            else
            {
                s = (int)Stone.StoneX; // 1
            }

            if (s == board[0] && s == board[1] && s == board[2])
                return (Stone)s;
            if (s == board[3] && s == board[4] && s == board[5])
                return (Stone)s;
            if (s == board[6] && s == board[7] && s == board[8])
                return (Stone)s;
            if (s == board[0] && s == board[3] && s == board[6])
                return (Stone)s;
            if (s == board[1] && s == board[4] && s == board[7])
                return (Stone)s;
            if (s == board[2] && s == board[5] && s == board[8])
                return (Stone)s;
            if (s == board[0] && s == board[4] && s == board[8])
                return (Stone)s;
            if (s == board[2] && s == board[4] && s == board[6])
                return (Stone)s;
        }
        return Stone.None;
    }

        void UpdateEnd()
    {
        

        if (stoneWinner != Stone.None)
        {
            Debug.Log("승리 : " + (int)stoneWinner);

            if (stoneWinner != Stone.StoneO)
            {
                Graphics.DrawTexture(new Rect(585, 415, 750, 250), winnerX);
                buttonRestart.gameObject.SetActive(true);
            }
            else if(stoneWinner != Stone.StoneX)
            {
                Graphics.DrawTexture(new Rect(585, 415, 750, 250), winnerO);
                buttonRestart.gameObject.SetActive(true);
            }
        }
        else
        {
            Graphics.DrawTexture(new Rect(585, 415, 750, 250), draw);
            buttonRestart.gameObject.SetActive(true);
            Debug.Log("무승부");

        }
    }

    bool SetStone(int i, Stone stone)
    {
        // 배치하려는 칸에 스톤이 배치되지 않았다면
        if (board[i] == (int)Stone.None)
        {
            // 칸에 스톤 배치
            board[i] = (int)stone;
            return true;
        }
        // 배치하려는 칸에 스톤이 배치되어 있으면 false 리턴
        return false;
    }


    int PosToNumber(Vector3 pos)
    {
        // 입력된 pos의 x좌표에서 50을 뺀 값을 x에 저장
        // 마우스 클릭한 위치에서 x좌표의 원점을 (50,0)으로 이동시키는 역할
        float x = pos.x - 660;
        // 입력된 pos의 y좌표에서 50을 뺀 값에 'Screen.height(화면높이)'에서 또 50을 뺀 값을 빼줌
        // 클릭 위치에서 y좌표의 원점을 ('Screen.height-50', 50)으로 이동시키는 역할
        float y = Screen.height - 300 - pos.y;

        if (x < 0.0f || x >= 600.0f)
        {
            return -1;
        }
        if (y < 0.0f || y >= 600.0f)
        {
            return -1;
        }

        // x 값을 100으로 나누어 몫을 정수형으로 변환하여 h에 저장
        // => x 좌표가 0부터 299까지의 범위에서 0,1,2로 매핑됨
        int h = (int)(x / 200.0f);
        int v = (int)(y / 200.0f);

        int i = v * 3 + h;

        return i;
    }


    bool MyTurn()
    {
        // 마우스 왼쪽 버튼이 클릭되었는지 여부를 확인
        bool bClick = Input.GetMouseButtonDown(0);

        // 클릭이 없으면 함수 종료
        if (!bClick)
        {
            return false;
        }

        // 마우스 클릭 위치를 화면 좌표로 저장
        Vector3 pos = Input.mousePosition;
       
        // 화면 좌표를 게임 보드 상의 위치로 변환
         int i = PosToNumber(pos);
       

        // 변환된 위치가 유효하지 않으면 함수 종료
        if (i == -1)
        {
            return false;
        }

        // 게임 보드에 돌을 놓음
        bool bSet = SetStone(i, stoneI);
        // 돌을 놓는 데 실패하면 함수 종료
        if (bSet == false) { return false; }

        // 길이가 1인 바이트 배열 data 생성
        byte[] data = new byte[1];
        // data 배열의 첫 번째 요소에 i값을 넣음(메시지로 보내기 위함)
        data[0] = (byte)i;
        tcp.Send(data, data.Length);

        Debug.Log("보냄:" + i);

        return true;
    }
    public void OnRestartButton()
    {
        // 초기화 코드 추가
        state = State.Start;
        stoneTurn = Stone.StoneO;
        stoneI = Stone.StoneO;
        stoneYou = Stone.StoneX;
        stoneWinner = Stone.None;

        for (int i = 0; i < board.Length; ++i)
        {
            board[i] = (int)Stone.None;
        }

        // 추가한 코드: 다시 그려진 texO, texX를 없앰
        // 여기서는 해당 텍스쳐가 그려진 상태에서만 없애는 코드입니다.
        // 다른 경우에는 상황에 따라 다르게 처리해야 할 수 있습니다.
        // 예를 들어, 상대방과의 통신이 이루어진 경우에는 서버와 클라이언트를 재시작해야 할 수 있습니다.
        // 상황에 맞게 수정해주세요.
        Graphics.DrawTexture(new Rect(660, 300, 600, 600), texBoard);

        buttonRestart.gameObject.SetActive(false);
    }
}