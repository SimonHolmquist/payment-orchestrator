# Guarda esto como Get-SlnTree.ps1 o pégalo en tu terminal
function Get-SlnTree {
    param(
        [string]$SlnPath = (Get-ChildItem *.sln | Select-Object -First 1 -ExpandProperty FullName)
    )

    if (-not $SlnPath -or -not (Test-Path $SlnPath)) {
        Write-Error "No se encontró ningún archivo .sln en la carpeta actual."
        return
    }

    $content = Get-Content $SlnPath
    $projects = @{}

    # 1. Extraer Proyectos y Carpetas de Solución
    # Formato: Project("{TypeGUID}") = "Name", "Path", "{ProjectGUID}"
    $projectRegex = 'Project\("\{.*?\}"\)\s*=\s*"(.*?)",\s*"(.*?)",\s*"\{(.*?)\}"'
    
    foreach ($line in $content) {
        if ($line -match $projectRegex) {
            $name = $matches[1]
            $path = $matches[2]
            $id = $matches[3]
            
            # TypeGUID para Carpetas de Solución suele ser 2150E333-8FDC-42A3-9474-1A3956D46DE8
            # Pero lo trataremos genéricamente por ahora.
            
            $projects[$id] = [PSCustomObject]@{
                Name = $name
                Path = $path
                Id = $id
                Children = @()
                IsRoot = $true # Asumimos root hasta que se demuestre lo contrario
            }
        }
    }

    # 2. Extraer Relaciones (Quién está dentro de quién)
    # Buscamos la sección GlobalSection(NestedProjects)
    $inNestedSection = $false
    foreach ($line in $content) {
        if ($line -match "GlobalSection\(NestedProjects\)") { $inNestedSection = $true; continue }
        if ($line -match "EndGlobalSection") { $inNestedSection = $false }

        if ($inNestedSection -and $line -match '\{(.*?)\}\s*=\s*\{(.*?)\}') {
            $childId = $matches[1]
            $parentId = $matches[2]

            if ($projects.ContainsKey($childId) -and $projects.ContainsKey($parentId)) {
                $projects[$parentId].Children += $projects[$childId]
                $projects[$childId].IsRoot = $false
            }
        }
    }

    # 3. Función recursiva para dibujar el árbol
    function Show-Tree {
        param($Nodes, $Indent, $IsLast)
        
        $count = $Nodes.Count
        $i = 0
        
        foreach ($node in $Nodes) {
            $i++
            $lastItem = ($i -eq $count)
            
            if ($lastItem) { $marker = "\---"; $subIndent = $Indent + "    " }
            else           { $marker = "+---"; $subIndent = $Indent + "|   " }

            # Determinar si es carpeta (si el path es el mismo nombre o no tiene extensión de proyecto)
            $isFolder = ($node.Path -eq $node.Name) -or ($node.Path -notmatch "\..+$")
            $color = if ($isFolder) { "Yellow" } else { "White" }
            
            Write-Host "$Indent$marker $($node.Name)" -ForegroundColor $color

            if ($node.Children.Count -gt 0) {
                # Ordenar: Carpetas primero, luego archivos, alfabéticamente
                $sortedChildren = $node.Children | Sort-Object { 
                    $isChildFolder = ($_.Path -eq $_.Name) -or ($_.Path -notmatch "\..+$")
                    -not $isChildFolder 
                }, Name

                Show-Tree -Nodes $sortedChildren -Indent $subIndent
            }
        }
    }

    # 4. Iniciar renderizado desde los elementos raíz
    $rootNodes = $projects.Values | Where-Object { $_.IsRoot } | Sort-Object { 
        $isFolder = ($_.Path -eq $_.Name) -or ($_.Path -notmatch "\..+$")
        -not $isFolder 
    }, Name
    
    Show-Tree -Nodes $rootNodes -Indent ""
}

# Ejecutar
Get-SlnTree