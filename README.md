# SoundPanel
Small sample player as a windows desktop application using WPF and NAudio. I've created this for practise purposes to get to know WPF.
It's trying to mimic some basic functionalities of software samplers like Steinberg's "Groove Agent" like loading samples through drag & drop,
saving/loading presets, learning keyboard/MIDI assignments.

KNOWN ISSUES:
- Pressing and releasing a MIDI note triggers a sample. It registers both NoteOn and NoteOff.
  (Tried to fix it with a toggle but the MIDI On and Off signal coming from my MIDI controller seems to act somewhat random)
