// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#ifndef __IMPORTER_UTILS_H__
#define __IMPORTER_UTILS_H__

void ReplaceCharacter(std::string& name, char c, char replacement)
{
	int nextCharacterPos = name.find(c);
	while (nextCharacterPos != std::string::npos)
	{
		name.replace(nextCharacterPos, 1, 1, replacement);
		nextCharacterPos = name.find(c, nextCharacterPos);
	}
}

void RemoveCharacter(std::string& name, char c)
{
	int nextCharacterPos = name.find(c);
	while (nextCharacterPos != std::string::npos)
	{
		name.erase(nextCharacterPos, 1);
		nextCharacterPos = name.find(c, nextCharacterPos);
	}
}

#endif // __IMPORTER_UTILS_H__