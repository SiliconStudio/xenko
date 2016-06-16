local typedefs = ""
local variables = ""
local getsyms = ""

function parseLine(line)
	local resultType, funcName, funcArgs = string.match(line, "^OVR_PUBLIC_FUNCTION%s*%(([%w_%s%*]+)%)%s*([%w_]+)%s*(.*)")
	if resultType == nil then return end
	typedefs = typedefs.."typedef "..resultType.." (*"..funcName.."Ptr) "..funcArgs.."\n"
	variables = variables..funcName.."Ptr "..funcName.."Func = nullptr;\n"
	getsyms = getsyms..funcName.."Func = ("..funcName.."Ptr)".."GetSymbolAddress(__libOvr, \""..funcName.."\");\n"
	getsyms = getsyms.."if(!"..funcName.."Func) { printf(\"Failed to get "..funcName.."\\n\"); return false; }\n"
end

io.input("Include/OVR_CAPI.h")

while true do
	local line = io.read()
	if line == nil then break end
	parseLine(line)
end

io.output("wrapper.c")
io.write(typedefs)
io.write(variables)
io.write(getsyms)