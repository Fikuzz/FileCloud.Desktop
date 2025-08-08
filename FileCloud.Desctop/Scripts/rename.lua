function rename_file(file)
    local currentDateTime = os.date("%Y-%m-%d_%H-%M-%S")
    local name, ext = string.match(file, "^(.*)%.(.*)$")
    local newName = name .. "_" .. currentDateTime
    return newName .. "." .. ext
end