using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : MonoBehaviour
{
    // Start is called before the first frame update
    public static int[] GetOffset(string pos)
    {
        int[] offset;

        if (pos[0] == 'b')
        {
            return new int[]{-15, 15, -17, 17, -6, 10};
        }
        else if (pos[0] == 'g')
        {
            return new int[]{-15, 15, -17, 17, 6, -10};
        }
        else if (pos[0] == 'h')
        {
            return new int[]{-15, 15, 6, -6, 10, -10};
        }
        else if (pos[0] == 'a')
        {
            return new int[]{-17, 17, 6, -6, 10, -10};
        }
        else
        {
            return new int[]{-15, 15, -17, 17, 6, -6, 10, -10};
        }

    }
}
