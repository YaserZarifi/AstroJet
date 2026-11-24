namespace Rake2D;

public enum TargetClass
{
	None = 0,
	EnemyGround = 8,
	EnemyAir = 16,
	Boss = 32,
	Man = 64,
	Building = 128,
	ShotEnemy = 256,
	MissileEnemy = 512,
	ShotHero = 1024,
	MissileHero = 2048,
	Secret = 4096,
	Other = 8192,
	Hero = 16384,
	Bonus = 32768,
	Ground = 65536,
	Drone = 131072,
	AllEnemy = 56,
	AllHero = 150528,
	Undefined = 1073741824
}
