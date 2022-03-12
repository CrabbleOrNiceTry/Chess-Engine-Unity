using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class MoveGenerationTest : MonoBehaviour
{
    private bool f;
    string universalString;

    // Start is called before the first frame update
    void Start()
    {
        f = false;
        universalString = "";
        // Debug.Log("1 ply: " + TestMoveGeneration(1));
        // Debug.Log("2 ply: " + TestMoveGeneration(2));
        // Debug.Log("3 ply: " + TestMoveGeneration(3));
        // Debug.Log("4 ply: " + TestMoveGeneration(4));
    }

    private int TestMoveGeneration(int depth, int originalDepth)
    {
        if (depth == 0)
        { return 1; }
        Move[] moves = GameManager.instance.board.GetLegalMoves();
        int sum = 0;
        foreach (Move move in moves)
        {
            GameManager.instance.board.MakeMove(move);
            int tempSum = TestMoveGeneration(depth - 1, originalDepth);
            // if (depth == originalDepth)
            // {
            //     universalString += move.ToString() + ": " + tempSum + "\n";
            // }
            sum += tempSum;
            GameManager.instance.board.UnmakeMove(move);
        }
        return sum;
    }


    void Update()
    {
        if (!f)
        {
            for (int i = 1; i <= 5; i++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var foo = TestMoveGeneration(i, i);
                watch.Stop();
                Debug.Log("At " + i + "ply: " + foo + " nodes. Operation performed in " + watch.ElapsedMilliseconds + "ms.");
            }
            // File.WriteAllText("Assets/Resources/MoveGenerationTest.txt", universalString);
            // int sum = 0;
            // for (int i = 1; i <= 4; i++)
            // {
            //     sum = TestMoveGeneration(i, i);
            //     Debug.Log((string)(i + "ply: " + sum + " moves " + watch.ElapsedMilliseconds + " ms"));
            // }
            f = true;
        }
    }

}
