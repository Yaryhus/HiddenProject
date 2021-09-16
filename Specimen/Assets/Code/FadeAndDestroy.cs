using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeAndDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    Color color = new Color(0, 0, 0, 0);
    float fadeSpeed = 0.05f;
    WaitForSeconds wait;

    void Start()
    {
        //Le dejamos sin colision y sin poder ser re-cortado.
        wait = new WaitForSeconds(5.0f);
        //color = this.GetComponent<MeshRenderer>().material.color;
        Destroy(gameObject, 10.0f);
    }

    // Update is called once per frame
    void Update()
    {
        //Hacemos transparente el objeto con el tiempo. Actualmente no funciona con el shader del slice asi que se quedará comentado.
        //color.a -= Time.deltaTime * fadeSpeed;
        //this.GetComponent<MeshRenderer>().material.color = color;
        
        if(transform.localScale.y >= 0.05f)
        transform.localScale += new Vector3(0.1F, .1f, .1f) * -2.0f * Time.deltaTime;  
    }

    //Quitamos etiqueta sliceable despues de 5 segundos para evitar problemas de performance con objetos que ya han sido destruidos.
    IEnumerator LoseSliceableTag()
    {
        yield return wait;
        gameObject.layer = 0;
    }
}
