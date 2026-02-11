using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script deifnido com o principal objetivo de posicionar a câmara do jogador de modo a que esta apresente
//um posicionamento semelhante a videojogos como Diablo 4, onde o jogador é capaz de visualizar a sua personagem por completo
//ao mesmo tempo que a câmara se situa relativamente afastada e em cima do mesmo.
public class CameraFollow : MonoBehaviour
{
    public Transform target; // O jogador que a câmera seguirá
    public Vector3 offset; // Distância da câmera em relação ao jogador
    public Vector3 cameraRotation;

    void LateUpdate()
    {
        if (target != null)
        {
            // Define a posição da câmera, somando a posição do jogador com o 'offset'
            transform.position = target.position + offset;

            // Mantém a rotação original da câmera
            transform.rotation = Quaternion.identity;

            // Define a rotação X da câmera para 45 graus
            transform.rotation *= Quaternion.Euler(cameraRotation.x, cameraRotation.y, cameraRotation.z);
        }
    }
}
