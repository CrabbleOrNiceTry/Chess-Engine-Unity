using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook
{

    public static int[] GetOffset(int index)
    {
        return new int[10];
    }

    public static int[] GetHorizontalOffset(Square square)
    {

        if (square.position[0] == 'a')
        {
            return new int[]{1};
        }
        else if (square.position[0] == 'h')
        {
            return new int[]{-1};
        }
        else
        {
            return new int[]{1, -1};
        }

    }
}
