#include <string>

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