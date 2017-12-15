using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameSpeedController : MonoBehaviour
{
	public void ChangeSpeed()
	{
		float val = GetComponent<Slider>().value;
		Image image = transform.GetChild(1).GetChild(0).GetComponent<Image>();
		Color c = image.color;

		if (val - 0.25f <= 1 && val + 0.25f >= 1)
		{
			val = 1;
			GetComponent<Slider>().value = val;
			c.a = 0;
		}
		else
			c.a = 255;

		image.color = c;
		Time.timeScale = val;
		transform.GetChild(3).GetComponent<Text>().text = "Speed: " + string.Format("{0:0.00}", val) + "x";
	}
}
