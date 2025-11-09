
using UnityEngine;

public class RandomIngredient : MonoBehaviour
{
    public PotController potController;

    [Header("Ingredients")]
    [SerializeField] public GameObject[] ingredients;

    [Header("Spawn Points")]
    [SerializeField] public GameObject[] spawnPoints;



    void Start()
    {

        RandomIngredientSpawn();

    }


    //-----------------------------------------------------------------------------------------------------------------------
    void RandomIngredientSpawn()
    {

        //Check if the list from PotController script is null or not (prolly not tho)
        if (potController.allPossibleIngredients == null)
        {
            Debug.LogError("allPossibleIngredients is null.");
            return;
        }

        //Shuffle the position of ingredients
        RandomIngredientIndex();

        //Spawn the ingredients
        for (int i = 1; i < potController.allPossibleIngredients.Count; i++)
        {
            Instantiate(ingredients[i], spawnPoints[i].GetComponent<Transform>().transform.position, Quaternion.identity);
        }
    }

    private void RandomIngredientIndex()
    {
        int ingredientCount = potController.allPossibleIngredients.Count;

        //Shuffle the index(spawnPoints)

        for (int i = ingredientCount - 1; i > 0; i--)
        {
            // Pick a random index from 0 to i (inclusive).
            int randomPosition = Random.Range(0, 13);

            // Swap the element at the current index i with the element at the random index.
            Transform temp = spawnPoints[i].GetComponent<Transform>().transform;
            spawnPoints[i] = spawnPoints[randomPosition];
            spawnPoints[randomPosition].GetComponent<Transform>().transform.position = temp.transform.position;
        }
    }
}
