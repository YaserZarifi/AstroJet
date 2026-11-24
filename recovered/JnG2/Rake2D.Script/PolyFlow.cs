using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class PolyFlow : BehaviorScript
{
	private Vector2 _flowSpeed;

	private int _fadeMode;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (_doInEditor && GameCore.IsEditorEnabled)
		{
			MyObject.OnEditorUpdate += OnEditorUpdate;
		}
		SetupVertices();
	}

	public override void Init()
	{
		base.Init();
		_flowSpeed = ParamVector2("flow_speed", 100f, 100f) / -100f;
		_fadeMode = ParamInt("fade_mode", 1);
		MyObject.PolygonRenderer.TextureFlowOffset = ParamRandomVector2Range("flow_start_offset") / 100f;
	}

	private void SetupVertices()
	{
		switch (_fadeMode)
		{
		case 1:
		{
			for (int j = 0; j < MyObject.PolygonRenderer.VertexList.Count; j++)
			{
				PolygonVertex polygonVertex2 = MyObject.PolygonRenderer.VertexList[j];
				if (j == 0 || j == 1)
				{
					polygonVertex2.Color = new Color(1f, 1f, 1f, 0f);
				}
				else
				{
					polygonVertex2.Color = Color.White;
				}
			}
			break;
		}
		case 2:
		{
			for (int k = 0; k < MyObject.PolygonRenderer.VertexList.Count; k++)
			{
				PolygonVertex polygonVertex3 = MyObject.PolygonRenderer.VertexList[k];
				if (k == 0 || k == 1 || k == MyObject.PolygonRenderer.VertexList.Count - 1 || k == MyObject.PolygonRenderer.VertexList.Count - 2)
				{
					polygonVertex3.Color = new Color(1f, 1f, 1f, 0f);
				}
				else
				{
					polygonVertex3.Color = Color.White;
				}
			}
			break;
		}
		case 3:
		{
			for (int i = 0; i < MyObject.PolygonRenderer.VertexList.Count; i++)
			{
				PolygonVertex polygonVertex = MyObject.PolygonRenderer.VertexList[i];
				float alpha = (float)(i / 2) / (float)((MyObject.PolygonRenderer.VertexList.Count - 2) / 2);
				polygonVertex.Color = new Color(1f, 1f, 1f, alpha);
			}
			break;
		}
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		MyObject.PolygonRenderer.TextureFlowOffset += _flowSpeed * elapsedTime;
	}

	private void OnEditorUpdate(float elapsedTime)
	{
		SetupVertices();
		MyObject.PolygonRenderer.TextureFlowOffset += _flowSpeed * elapsedTime;
	}
}
