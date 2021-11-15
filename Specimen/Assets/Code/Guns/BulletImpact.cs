using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    //Insert more materials here and put them in a switch case in ChangeVisuals depending on the material
    [Header("Materials")]
    [SerializeField]
    Material fleshMaterial;
    [SerializeField]
    Material snowMaterial;
    [SerializeField]
    Material concreteMaterial;
    [SerializeField]
    Material woodMaterial;
    [SerializeField]
    Material sandMaterial;
    [SerializeField]
    Material metalMaterial;
    [SerializeField]
    Material waterMaterial;
    [SerializeField]
    Material clothMaterial;
    [SerializeField]
    Material cristalMaterial;
    [SerializeField]
    Material mudMaterial;
    [SerializeField]
    Material grassMaterial;
    [SerializeField]
    Material paperboardMaterial;
    [SerializeField]
    Material iceMaterial;

    [Header("Particles")]
    [SerializeField]
    ParticleSystem fleshParticle;
    [SerializeField]
    ParticleSystem snowParticle;
    [SerializeField]
    ParticleSystem concreteParticle;
    [SerializeField]
    ParticleSystem woodParticle;
    [SerializeField]
    ParticleSystem sandParticle;
    [SerializeField]
    ParticleSystem metalParticle;
    [SerializeField]
    ParticleSystem waterParticle;
    [SerializeField]
    ParticleSystem clothParticle;
    [SerializeField]
    ParticleSystem cristalParticle;
    [SerializeField]
    ParticleSystem mudParticle;
    [SerializeField]
    ParticleSystem grassParticle;
    [SerializeField]
    ParticleSystem paperboardParticle;
    [SerializeField]
    ParticleSystem iceParticle;

    // Start is called before the first frame update
    void Start()
    {
        transform.Rotate(0.0f, 0.0f, Random.Range(0, 360));
    }

    public void ChangeVisuals(System.String matName)
    {
        //Depending what impatcs the material is different
        switch (matName)
        {
            case "Cloth":
                GetComponent<MeshRenderer>().material = clothMaterial;
                clothParticle.gameObject.SetActive(true);
                clothParticle.Play();
                break;
            case "Concrete":
                GetComponent<MeshRenderer>().material = concreteMaterial;
                concreteParticle.gameObject.SetActive(true);
                concreteParticle.Play();
                break;
            case "Cristal":
                GetComponent<MeshRenderer>().material = cristalMaterial;
                cristalParticle.gameObject.SetActive(true);
                cristalParticle.Play();
                break;
            case "Flesh":
                GetComponent<MeshRenderer>().material = fleshMaterial;
                fleshParticle.gameObject.SetActive(true);
                fleshParticle.Play();
                break;
            case "Grass":
                GetComponent<MeshRenderer>().material = grassMaterial;
                grassParticle.gameObject.SetActive(true);
                grassParticle.Play();
                break;
            case "Ice":
                GetComponent<MeshRenderer>().material = iceMaterial;
                iceParticle.gameObject.SetActive(true);
                iceParticle.Play();
                break;
            case "Metal":
                GetComponent<MeshRenderer>().material = metalMaterial;
                metalParticle.gameObject.SetActive(true);
                metalParticle.Play();
                break;
            case "Mud":
                GetComponent<MeshRenderer>().material = mudMaterial;
                mudParticle.gameObject.SetActive(true);
                mudParticle.Play();
                break;
            case "PaperBoard":
                GetComponent<MeshRenderer>().material = paperboardMaterial;
                paperboardParticle.gameObject.SetActive(true);
                paperboardParticle.Play();
                break;
            case "Snow":
                GetComponent<MeshRenderer>().material = snowMaterial;
                snowParticle.gameObject.SetActive(true);
                snowParticle.Play();
                break;
            case "Water":
                GetComponent<MeshRenderer>().material = waterMaterial;
                waterParticle.gameObject.SetActive(true);
                waterParticle.Play();
                break;
            case "Wood":
                GetComponent<MeshRenderer>().material = woodMaterial;
                woodParticle.gameObject.SetActive(true);
                woodParticle.Play();
                break;
            case "Sand":
                GetComponent<MeshRenderer>().material = sandMaterial;
                sandParticle.gameObject.SetActive(true);
                sandParticle.Play();
                break;
            default:
                break;


        }
    }
}
