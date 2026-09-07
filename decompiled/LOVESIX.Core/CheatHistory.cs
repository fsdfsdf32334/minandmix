using System;
using System.Collections.Generic;
using System.Linq;

namespace LOVESIX.Core;

/// <summary>
/// Tracks cheat history with undo/redo capabilities
/// </summary>
public class CheatHistory
{
    private Stack<HistoryState> _undoStack = new();
    private Stack<HistoryState> _redoStack = new();
    private List<HistoryEntry> _history = new();
    
    public event Action HistoryChanged;
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    
    public void RecordState(string cheatName, bool enabled, string details = "")
    {
        var entry = new HistoryEntry
        {
            Timestamp = DateTime.Now,
            CheatName = cheatName,
            Enabled = enabled,
            Details = details
        };
        
        _history.Add(entry);
        _redoStack.Clear();
        HistoryChanged?.Invoke();
    }
    
    public List<HistoryEntry> GetHistory(int take = 50)
    {
        return _history.OrderByDescending(h => h.Timestamp).Take(take).ToList();
    }
    
    public void Clear()
    {
        _history.Clear();
        _undoStack.Clear();
        _redoStack.Clear();
        HistoryChanged?.Invoke();
    }
}

public class HistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string CheatName { get; set; }
    public bool Enabled { get; set; }
    public string Details { get; set; }
}

public class HistoryState
{
    public Dictionary<string, bool> CheatStates { get; set; }
    public DateTime CreatedAt { get; set; }
}
