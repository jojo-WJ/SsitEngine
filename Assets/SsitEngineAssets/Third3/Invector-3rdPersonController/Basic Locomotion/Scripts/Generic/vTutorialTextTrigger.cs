using UnityEngine;
using UnityEngine.UI;

public class vTutorialTextTrigger : MonoBehaviour
{
    public Text _textUI;
    public GameObject painel;

    [TextArea(5, 3000)] [Multiline] public string text;

    private void OnTriggerEnter( Collider other )
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            painel.SetActive(true);
            _textUI.gameObject.SetActive(true);
            _textUI.text = text;
        }
    }

    private void OnTriggerExit( Collider other )
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            painel.SetActive(false);
            _textUI.gameObject.SetActive(false);
            _textUI.text = " ";
        }
    }
}