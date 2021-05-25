using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : MonoBehaviour
{
    // Start is called before the first frame update
    public static int[] GetOffset(Square square)
    {   
        // Comment positions are relative to white on bottom of board

        // Bottom left Corner
        if (square.position[0] == 'a' && square.position[1] == '1')
        {
            return new int[] {1, -7, -8};
        }
        // Bottom right Corner
        else if (square.position[0] == 'h' && square.position[1] == '1')
        {
            return new int[] {-1, -9, -8};
            
        }
        // Upper left Corner
        else if (square.position[0] == 'a' && square.position[1] == '8')
        {
            return new int[] {-1, 8, 9};
        }
        // Upper right Corner
        else if (square.position[0] == 'h' && square.position[1] == '8')
        {
            return new int[] {-1, 8, 7};
        }
        //Bottom 
        else if (square.position[1] == '1')
        {
            return new int[] {1, -1, -8, -9, -7};
        }
        // Upper 
        else if (square.position[1] == '8')
        {
            return new int[] {1, -1, 8, 9, 7};
        }
        // On left side
        else if (square.position[0] == 'a')
        {
            return new int[] {1, 8, -8, -7, 7};
        }
        // On right side
        else if (square.position[0] == 'h')
        {
            return new int[] {-1, 8, -8, -9, 9};
        }
        else
        {
            return new int[] {1, -1, 8, -8, 7, -7, 9, -9};
        }
    } 
}
