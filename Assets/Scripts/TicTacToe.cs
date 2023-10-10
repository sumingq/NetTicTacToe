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

    // ��ſ� ���� ����
    Tcp tcp;
    public InputField ip;

    // Texture ���� ����
    public Texture texBoard;
    public Texture texX;
    public Texture texO;

    public Texture turnX;
    public Texture turnO;

    public Texture winnerX;
    public Texture winnerO;
    public Texture draw;

    // 3*3 ���� �迭 ����
    int[] board = new int[9];
    // ����Ʈ�� ũ�Ⱑ ������ ���� �ʾƼ� ���������� ��� ����
    // �迭�� �����ϸ� ũ�⸦ �����ϱ� ���� �ڵ带 ������ �����ؾ� ��

    // ���� ���� ������ ����
    State state;

    // ǥ���� ����, �� ����, ��� ����, ����
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
        // ���� ���� ������Ʈ�� �پ� �ִ� 'TCP' ������Ʈ�� �ν��Ͻ��� tcp������ �Ҵ�
        tcp = GetComponent<Tcp>();
        // ������ ���۵� �� state ������ State.Start�� �ʱ�ȭ
        state = State.Start;
        // board �迭�� �� ��Ҹ� None���� �ʱ�ȭ
        for (int i = 0; i < board.Length; ++i)
        {
            // enum Stone���� None�� �ش��ϴ� ���� ������ ��ȯ�� ��
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

        // ������ ���۵Ǹ� ����, Ŭ���̾�Ʈ ��ư�� InputField�� ��Ȱ��ȭ
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

        // Ŭ���̾�Ʈ�� ����Ǹ� ����, Ŭ���̾�Ʈ ��ư�� InputField�� ��Ȱ��ȭ
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


    //  �� �����Ӹ��� ȣ�� (GUI�� �׸��� ���� �ڵ����� ȣ��Ǵ� �޼���)
    void OnGUI()
    {
        // Repaint �̺�Ʈ�� �߻��� ������ ȭ���� ������ ������ 'texBoard'��� �ؽ��ĸ� �׸�
        if(!Event.current.type.Equals(EventType.Repaint)) return;
        // ���� �� �𼭸�(660,300)���� �����Ͽ� ���� 600�ȼ�, ���� 600�ȼ��� �簢�� ������ �ؽ��ĸ� �׸�
        Graphics.DrawTexture(new Rect(660, 300, 600, 600), texBoard);


        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] != (int)Stone.None)
            {
                // i�� ���� ������ ���� 195�� ���� (�� �ؽ����� ����)
                // ��ġ�� ������ ����� 30��ŭ �̵����Ѽ� �������� ��ǥ ���
                float x = 30 + 660 + (i % 3) * 195;
                float y = 30 + 300 + (i / 3) * 195;

                Texture tex = (board[i] == (int)Stone.StoneO) ? texO : texX;
                // ���� ��ġ�� 150x150 ũ���� �ؽ��ĸ� �׸�
                Graphics.DrawTexture(new Rect(x, y, 150, 150), tex);
            }
        }

        // ���� ���� ����
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

        // ���� ���� ����
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
        // ���� ��� ���·� ����
        state = State.Game;
        // O ���� ����
        stoneTurn = Stone.StoneO;

        // ����(��) ǥ�� = O
        if (tcp.IsServer())
        {
            stoneI = Stone.StoneO;
            stoneYou = Stone.StoneX;
        }

        // Ŭ���̾�Ʈ(���) ǥ�� = X
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
            Debug.Log("�¸�: " + (int)stoneWinner);
        }

        else if (CheckDraw()) // ���º� üũ
        {
            state = State.End;
            Debug.Log("���º�");
        }

        stoneTurn = (stoneTurn == Stone.StoneO) ? Stone.StoneX : Stone.StoneO;
    }

    bool CheckDraw()
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == (int)Stone.None)
            {
                return false; // ���� �� �ڸ��� ������ ��� ����
            }
        }
        return true; // ��� �ڸ��� ���� ���º�
    }


    //Ŭ���̾�Ʈ�� �����κ��� ���� ������ �̿��Ͽ� ������ �� ���� �ڽ��� ���� ���忡 ǥ��
    bool YourTurn()
    {
        // ũ�Ⱑ 1�� ����Ʈ �迭 'data'�� ����, �������� ���� �����͸� ������ �뵵
        byte[] data = new byte[1];
        // tcp.Receive �޼��带 ȣ���Ͽ� �����κ��� �����͸� �޾ƿ�
        int iSize = tcp.Receive(ref data, data.Length);

        if (iSize <= 0)
        {
            return false;
        }

        // ���ŵ� ������ �迭�� ù ��° ��Ҹ� ���������� ��ȯ�Ͽ� 'i'�� ����
        int i = (int)data[0];
        Debug.Log("����:" + i);

        // ���� �����͸� �̿��Ͽ� ������ �� ���� ���Ӻ��忡 ��ġ�ϴ� 'SetStone' �޼��带 ȣ��
        bool ret = SetStone(i, stoneYou);
        if(ret == false)
        {
            return false;
        }
        return true;
    }

    Stone CheckBoard()
    {
        // �ΰ����� �� ����(StoneO,StoneX)�� ���� ���� �¸� ���θ� üũ
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
            Debug.Log("�¸� : " + (int)stoneWinner);

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
            Debug.Log("���º�");

        }
    }

    bool SetStone(int i, Stone stone)
    {
        // ��ġ�Ϸ��� ĭ�� ������ ��ġ���� �ʾҴٸ�
        if (board[i] == (int)Stone.None)
        {
            // ĭ�� ���� ��ġ
            board[i] = (int)stone;
            return true;
        }
        // ��ġ�Ϸ��� ĭ�� ������ ��ġ�Ǿ� ������ false ����
        return false;
    }


    int PosToNumber(Vector3 pos)
    {
        // �Էµ� pos�� x��ǥ���� 50�� �� ���� x�� ����
        // ���콺 Ŭ���� ��ġ���� x��ǥ�� ������ (50,0)���� �̵���Ű�� ����
        float x = pos.x - 660;
        // �Էµ� pos�� y��ǥ���� 50�� �� ���� 'Screen.height(ȭ�����)'���� �� 50�� �� ���� ����
        // Ŭ�� ��ġ���� y��ǥ�� ������ ('Screen.height-50', 50)���� �̵���Ű�� ����
        float y = Screen.height - 300 - pos.y;

        if (x < 0.0f || x >= 600.0f)
        {
            return -1;
        }
        if (y < 0.0f || y >= 600.0f)
        {
            return -1;
        }

        // x ���� 100���� ������ ���� ���������� ��ȯ�Ͽ� h�� ����
        // => x ��ǥ�� 0���� 299������ �������� 0,1,2�� ���ε�
        int h = (int)(x / 200.0f);
        int v = (int)(y / 200.0f);

        int i = v * 3 + h;

        return i;
    }


    bool MyTurn()
    {
        // ���콺 ���� ��ư�� Ŭ���Ǿ����� ���θ� Ȯ��
        bool bClick = Input.GetMouseButtonDown(0);

        // Ŭ���� ������ �Լ� ����
        if (!bClick)
        {
            return false;
        }

        // ���콺 Ŭ�� ��ġ�� ȭ�� ��ǥ�� ����
        Vector3 pos = Input.mousePosition;
       
        // ȭ�� ��ǥ�� ���� ���� ���� ��ġ�� ��ȯ
         int i = PosToNumber(pos);
       

        // ��ȯ�� ��ġ�� ��ȿ���� ������ �Լ� ����
        if (i == -1)
        {
            return false;
        }

        // ���� ���忡 ���� ����
        bool bSet = SetStone(i, stoneI);
        // ���� ���� �� �����ϸ� �Լ� ����
        if (bSet == false) { return false; }

        // ���̰� 1�� ����Ʈ �迭 data ����
        byte[] data = new byte[1];
        // data �迭�� ù ��° ��ҿ� i���� ����(�޽����� ������ ����)
        data[0] = (byte)i;
        tcp.Send(data, data.Length);

        Debug.Log("����:" + i);

        return true;
    }
    public void OnRestartButton()
    {
        // �ʱ�ȭ �ڵ� �߰�
        state = State.Start;
        stoneTurn = Stone.StoneO;
        stoneI = Stone.StoneO;
        stoneYou = Stone.StoneX;
        stoneWinner = Stone.None;

        for (int i = 0; i < board.Length; ++i)
        {
            board[i] = (int)Stone.None;
        }

        // �߰��� �ڵ�: �ٽ� �׷��� texO, texX�� ����
        // ���⼭�� �ش� �ؽ��İ� �׷��� ���¿����� ���ִ� �ڵ��Դϴ�.
        // �ٸ� ��쿡�� ��Ȳ�� ���� �ٸ��� ó���ؾ� �� �� �ֽ��ϴ�.
        // ���� ���, ������� ����� �̷���� ��쿡�� ������ Ŭ���̾�Ʈ�� ������ؾ� �� �� �ֽ��ϴ�.
        // ��Ȳ�� �°� �������ּ���.
        Graphics.DrawTexture(new Rect(660, 300, 600, 600), texBoard);

        buttonRestart.gameObject.SetActive(false);
    }
}