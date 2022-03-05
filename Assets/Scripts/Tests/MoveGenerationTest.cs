using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGenerationTest : MonoBehaviour
{
    private bool f;

    // Start is called before the first frame update
    void Start()
    {
        f = false;
        // Debug.Log("1 ply: " + TestMoveGeneration(1));
        // Debug.Log("2 ply: " + TestMoveGeneration(2));
        // Debug.Log("3 ply: " + TestMoveGeneration(3));
        // Debug.Log("4 ply: " + TestMoveGeneration(4));
    }

    private int TestMoveGeneration(int depth)
    {
        if (depth == 0)
        { return 1; }
        Move[] moves = GameManager.instance.board.GetLegalMoves();
        int sum = 0;
        foreach (Move move in moves)
        {
            GameManager.instance.board.MakeMove(move);
            sum += TestMoveGeneration(depth - 1);
            GameManager.instance.board.UnmakeMove(move);
        }
        return sum;
    }


    void Update()
    {
        if (!f)
        {
            TestMoveGeneration(1);
            int sum = 0;
            for (int i = 1; i <= 4; i++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                sum = TestMoveGeneration(i);
                watch.Stop();
                Debug.Log((string)(i + "ply: " + sum + " moves " + watch.ElapsedMilliseconds + " ms"));
            }
            f = true;
        }
    }

}
