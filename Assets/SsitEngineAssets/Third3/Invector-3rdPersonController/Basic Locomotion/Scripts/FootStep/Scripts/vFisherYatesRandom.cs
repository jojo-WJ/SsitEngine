using UnityEngine;

// Fisher-Yates shuffle -- makes sure all items are selected with equal probability and that the same item is not selected twice in a row.
public class vFisherYatesRandom
{
    private int prevValue = -1;
    private int randomIndex;
    private int[] randomIndices;

    public int Next( int len )
    {
        if (len <= 1)
            return 0;

        if (randomIndices == null || randomIndices.Length != len)
        {
            randomIndices = new int[len];
            for (var i = 0; i < randomIndices.Length; i++)
                randomIndices[i] = i;
        }

        if (randomIndex == 0)
        {
            var count = 0;
            do
            {
                for (var i = 0; i < len - 1; i++)
                {
                    var j = Random.Range(i, len);
                    if (j != i)
                    {
                        var tmp = randomIndices[i];
                        randomIndices[i] = randomIndices[j];
                        randomIndices[j] = tmp;
                    }
                }
            } while (prevValue == randomIndices[0] && ++count < 10
            ); // Make sure the new first element is different from the last one we played
        }

        var value = randomIndices[randomIndex];
        if (++randomIndex >= randomIndices.Length)
            randomIndex = 0;

        prevValue = value;
        return value;
    }
}