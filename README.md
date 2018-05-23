# Overview

This tool is related to the game Monster Hunter: World.

Its goal is to automatically produce a list of jewels, with the minimum amount to keep in order to be able to get the full skill level.

For example, the jewel `Antidote Jewel 1` grants the skill `Poison Resistance`, and this skill requires 3 points to reach its maximum level, meaning that you will never need more than 3 `Antidote Jewel 1`, and so if you have more then 3 jewels, you know how many you can meld to the Elder Melder.

# How it works

The tool fetches decorations and skills data from mhw-db.com, and for each decoration it finds out the associated skill, and from that skill it determines the amount of points needed to reach the maximum level, knowing that one jewel grants one point.
