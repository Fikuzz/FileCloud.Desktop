function rename_file(file)
    local name, ext = string.match(file, "^(.*)%.(.*)$")
    local newName = name .. "_" .. "Downloaded"
    return newName .. "." .. ext
end