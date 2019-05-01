﻿#region [Copyright (c) 2018 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2018 Cristian Alexandru Geambasu
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//	and associated documentation files (the "Software"), to deal in the Software without restriction, 
//	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all copies or substantial 
//	portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Luminosity.IO
{
    [RequireComponent(typeof(GenericGamepadStateAdapter))]
    public class GenericGamepadProfileSelector : MonoBehaviour
    {
        private enum QueryType
        {
            ExactMatch, StartsWith, EndsWith, Contains
        }

        [Serializable]
        private class Query
        {
            public QueryType Type = QueryType.Contains;
            public string Content = "";
        }

        [Serializable]
        private class ProfileEntry
        {
            public GenericGamepadProfile Profile = null;
            public List<Query> Constraints = null;
        }

        [SerializeField]
        private GenericGamepadProfile m_defaultProfile = null;
        [SerializeField]
        private List<ProfileEntry> m_entries = null;

        private GenericGamepadStateAdapter m_adapter;

        private void Start()
        {
            m_adapter = GetComponent<GenericGamepadStateAdapter>();
            m_adapter.GamepadConnected += AssignGamepadProfile;

            AssignGamepadProfile(GamepadIndex.GamepadOne);
            AssignGamepadProfile(GamepadIndex.GamepadTwo);
            AssignGamepadProfile(GamepadIndex.GamepadThree);
            AssignGamepadProfile(GamepadIndex.GamepadFour);
        }

        private void AssignGamepadProfile(GamepadIndex gamepad)
        {
            if(!m_adapter.IsConnected(gamepad))
                return;

            string gamepadName = m_adapter.GetName(gamepad);
            for(int i = 0; i < m_entries.Count; i++)
            {
                if(IsMatch(m_entries[i], gamepadName))
                {
                    m_adapter.SetProfile(gamepad, m_entries[i].Profile);
                    Debug.Log($"Profile '{m_entries[i].Profile.Name}' assigned to '{gamepadName}'.");
                    return;
                }
            }

            if(m_defaultProfile != null)
            {
                m_adapter.SetProfile(gamepad, m_defaultProfile);
                Debug.Log($"No profile found for '{gamepadName}'. Assigning default profile '{m_defaultProfile.Name}'.");
            }
            else
            {
                Debug.Log($"No profile found for '{gamepadName}'.");
            }
        }

        private bool IsMatch(ProfileEntry entry, string gamepadName)
        {
            for(int i = 0; i < entry.Constraints.Count; i++)
            {
                if(!IsMatch(entry.Constraints[i], gamepadName))
                    return false;
            }

            return true;
        }

        private bool IsMatch(Query query, string gamepadName)
        {
            StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

            switch(query.Type)
            {
            case QueryType.ExactMatch:
                return string.Compare(gamepadName, query.Content, comparison) == 0;
            case QueryType.StartsWith:
                return gamepadName.StartsWith(query.Content, comparison);
            case QueryType.EndsWith:
                return gamepadName.EndsWith(query.Content, comparison);
            case QueryType.Contains:
                return gamepadName.ToLowerInvariant().Contains(query.Content.ToLowerInvariant());
            default:
                return false;
            }
        }
    }
}
