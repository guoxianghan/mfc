using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Constants
{
    public static class SNTONMath
    {
        public static void BubbleSort(ref int[] arr)
        {
            int temp = 0;
            for (int i = 0; i < arr.Length - 1; i++)  //外层循环控制排序趟数
            {
                for (int j = 0; j < arr.Length - 1 - i; j++)  //内层循环控制每一趟排序多少次
                {
                    if (arr[j] > arr[j + 1])
                    {
                        temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
                }
            }
        }
    }
}
