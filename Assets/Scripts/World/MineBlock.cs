using UnityEngine;
using System.Collections;

public enum BlockType 
{
	Air=-2,
	Unknown=-1,
	Dirt=0,
	Grass=1,
	Stone=2,
	BedRock=3,
};

public class MineBlock : MonoBehaviour {

//	private static int _minLight = 51;
//	public static int MinLight { get { return _minLight; } }
//	
//	private static int _maxLight = 255;
//	public static int MaxLight { get { return _maxLight; } }
	
	private BlockType _type; 
	public BlockType Type {
		get { return _type; }
		set { 
			_type = value;
			switch(_type) {
			case BlockType.Air:
				solid = false;
				break;
			case BlockType.Unknown:
				solid = false;
				break;
			default:
				solid = true;
				break;
			}
		}
	}

	private bool solid;

//	private int _light;
//	public int Light 
//	{ 
//		get { return _light; }
//		set 
//		{ 
//			int light =  Mathf.Clamp(value, _minLight, _maxLight);
//			if (_light != light)
//			{
//				_light = light;
//			}
//		} 
//	}

	public MineBlock(BlockType type) {
		_type = type;
	}

	public bool IsSolid() {
		return solid;
	}
}
