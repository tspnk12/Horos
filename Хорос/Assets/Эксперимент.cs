using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Эксперимент : MonoBehaviour
{
    public float радиусКольца = 0.5f; // средний между внешним и внутренним
    public float массаКольца  = 1f;
    public float массаСтержня = 0.1f;
    public int   количествоСтержней = 3;
    public float длинаСтержня = 1f;
    public float начальныйПоворотКольца = 10f;
    [Space]
    public Rigidbody  потолок;
    public GameObject префаб_стержень;
    public Transform  префаб_сегментКольца;

    private Transform кольцо;
    private float стержниY; // Y-координата стержней

    //----------------------------------------------------------
    void Start()
    {
        // Создаем всю модель
        СоздатьКольцо();
        СоздатьСтержни();

        // Отодвигаем камеру, чтобы всё было видно
        Camera.main.transform.position = new Vector3(0, стержниY, -Mathf.Max(1.2f*длинаСтержня, радиусКольца*2));

        // Задаем кольцу начальные силы
        кольцо.rotation = Quaternion.Euler(0, начальныйПоворотКольца, 0);
    }
    //----------------------------------------------------------
    Transform СоздатьКольцо()
    {
        const int количествоСегментов = 60;

        // Пустой пока объект
        кольцо = new GameObject("Кольцо").transform;

        // Создаем сегменты кольца
        for (int i=0; i < количествоСегментов; i++)
        {
            // Спавним новый сегмент
            Transform сегмент = Instantiate(префаб_сегментКольца, кольцо, true);
            // Сдвигаем его на среднюю окружность кольца
            сегмент.position = new Vector3(0, 0, 0.5f);
            // Поворачиваем кольцо, чтобы освободить место для следующего сегмента
            кольцо.Rotate(0, 360/количествоСегментов, 0);
        }
        // Задаем кольцу нужный радиус
        кольцо.localScale = new Vector3(радиусКольца*2, 1, радиусКольца*2);

        // Добавляем кольцу физику
        var rb = кольцо.gameObject.AddComponent<Rigidbody>();
        rb.mass = массаКольца;
        rb.angularDrag = 0;

        return кольцо;
    }
    //----------------------------------------------------------
    void СоздатьСтержни()
    {
        // Вспомогательный группирующий объект для равномерного распределения стержней по кольцу
        Transform стержни = new GameObject("стержни").transform;

        // Расставляем стержни по окружности
        for (int i=0; i < количествоСтержней; i++)
        {
            // Спавним новый стержень
            var стержень = Instantiate(префаб_стержень, стержни);
            // Сдвигаем его на окружность
            стержень.transform.position = new Vector3(0, 0, радиусКольца);
            // Задаем массу
            стержень.GetComponent<Rigidbody>().mass = массаСтержня;
            // Поворачиваем всю группу стержней, чтобы обеспечить одинаковое расстояние между ними
            стержни.Rotate(0, 360/количествоСтержней, 0);
        }

        // Задаем стержням длину 
        Vector3 scale = стержни.localScale;
        scale.y = длинаСтержня;
        стержни.localScale = scale;

        // Задаем стержням нужное положение между потолком и кольцом
        float низПотолка = потолок.GetComponent<Collider>().bounds.min.y;
        float верхКольца = кольцо.GetComponentInChildren<Collider>().bounds.max.y;
        float нужныйСдвигКольца = низПотолка-верхКольца-длинаСтержня;
        кольцо.Translate(0, нужныйСдвигКольца, 0);
        стержниY = (низПотолка+верхКольца+нужныйСдвигКольца)/2f;
        стержни.position = new Vector3(0, стержниY, 0);

        // Присоединияем стержни к потолоку и кольцу
        foreach (Transform стержень in стержни)
        {
            // Крепление к потолку
            var joint = стержень.gameObject.AddComponent<ConfigurableJoint>();
            joint.anchor = new Vector3(0, 0.5f, 0); // самая верхняя точка стержня
            joint.connectedBody = потолок;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            // Крепление к кольцу
            joint = стержень.gameObject.AddComponent<ConfigurableJoint>();
            joint.anchor = new Vector3(0, -0.5f, 0); // самая нижняя точка стержня
            joint.connectedBody = кольцо.GetComponent<Rigidbody>();
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
        }
    }
    //----------------------------------------------------------
}
