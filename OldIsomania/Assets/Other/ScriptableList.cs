/*	MIT License

	Copyright (c) 2021 hibzz.games

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.
	
	---

	Author: Hibnu Hishath (sliptixx)

	Notes: Crediting in derived work is not necessary, but is greatly appreciated.

	Notes: Feel free to use it in your games/projects as is or in modified form. You
	may sell modified versions of this code, but try not to sell the code as is in the
	form of developer packages. This is licensed under MIT, so I can't stop you from 
	doing that, but that's just a dick move.
*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An empty interface that ensures that the generic Type T inherits from Scriptable objects
/// </summary>
public interface IScriptableList {}

/// <summary>
/// Marks a list of scriptable objects to be drawn so
/// </summary>
[System.Serializable]
public class ScriptableList<T> : ISerializationCallbackReceiver, IScriptableList where T : ScriptableObject
{
	public List<T> items;

	public ScriptableList()
	{
	}

	public void OnAfterDeserialize()
	{
		if(items == null)
		{
			items = new List<T>();
		}
	}

	public void OnBeforeSerialize()
	{
	}
}
