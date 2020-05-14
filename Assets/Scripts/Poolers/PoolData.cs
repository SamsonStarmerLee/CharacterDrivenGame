﻿using UnityEngine;
using System.Collections.Generic;

namespace Pooling
{
	public class PoolData
	{
		public GameObject prefab;
		public int maxCount;
		public Queue<Poolable> pool;
	}
}