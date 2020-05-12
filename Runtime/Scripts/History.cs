﻿using System.Collections.Generic;
using System.Linq;
using ExpressoBits.Console.UI;
using UnityEngine;

namespace ExpressoBits.Console
{
    [RequireComponent(typeof(Commander))]
    public class History : MonoBehaviour
    {
        public KeyCode upKeyCode = KeyCode.UpArrow;
        public KeyCode downKeyCode = KeyCode.DownArrow;
        [Range(0, 256)] public int maxHistoryRegistry;
        public List<string> history;
        
        private int m_ActualIndex;
        private const int noValue = -1;
        private VisualConsole m_VisualConsole;
        private const string saveHistoryKey = "br.com.ExpressoBits.Console.History";
        private Commander m_Commander;
        
        private void Awake()
        {
            m_Commander = GetComponent<Commander>();
            m_VisualConsole = GetComponentInChildren<VisualConsole>();
            history = new List<string>();
            LoadHistory();
        }

        private void LoadHistory()
        {
            if (PlayerPrefs.HasKey(saveHistoryKey))
            {
                var raw = PlayerPrefs.GetString(saveHistoryKey);
                var commands = raw.Split('\n');
                foreach (var command in commands)
                {
                    history.Add(command);
                }
            }
        }

        private void Start()
        {
            m_Commander.onOpenCommander.AddListener(delegate { enabled = true; });
            m_Commander.onCloseCommander.AddListener(delegate { enabled = false; });
            
            
        }

        private void OnEnable()
        {
            m_Commander.onProcessCommand.AddListener(AddLastCommand);
        }

        private void OnDisable()
        {
            m_Commander.onProcessCommand.RemoveListener(AddLastCommand);
        }

        private void AddLastCommand()
        {
            if (m_VisualConsole.consoleInput.text.Length > 0)
            {
                history.Add(m_VisualConsole.consoleInput.text);
                if (history.Count > maxHistoryRegistry)
                {
                    history.RemoveAt(0);
                }

            }
            
            m_ActualIndex = noValue;
        }

        private void Update()
        {
            if (Input.GetKeyDown(downKeyCode))
            {
                if (history.Count == 0) return;
                m_ActualIndex++;
                if (m_ActualIndex == history.Count) m_ActualIndex = 0;
                m_VisualConsole.consoleInput.text = history[m_ActualIndex];
                m_VisualConsole.consoleInput.caretPosition = m_VisualConsole.consoleInput.text.Length;
            }

            else if (Input.GetKeyDown(upKeyCode))
            {
                if (history.Count == 0) return;
                m_ActualIndex--;
                switch (m_ActualIndex)
                {
                    case noValue:
                        m_VisualConsole.consoleInput.text = "";
                        return;
                    case -2:
                        m_ActualIndex = history.Count-1;
                        break;
                }

                m_VisualConsole.consoleInput.text = history[m_ActualIndex];
                m_VisualConsole.consoleInput.caretPosition = m_VisualConsole.consoleInput.text.Length;
            }
        }

        private void OnDestroy()
        {
            var value = history.Aggregate("", (current, command) => current + (command + "\n"));
            PlayerPrefs.SetString(saveHistoryKey,value);
            PlayerPrefs.Save();
        }
    }
}