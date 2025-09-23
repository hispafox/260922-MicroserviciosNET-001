# Tabla de operaciones JSON Patch

| Operación | Descripción | Ejemplo de uso |
|-----------|-------------|----------------|
| add       | Agrega un valor en la ruta indicada | `{ "op": "add", "path": "/name", "value": "Nuevo" }` |
| remove    | Elimina el valor en la ruta indicada | `{ "op": "remove", "path": "/name" }` |
| replace   | Reemplaza el valor en la ruta indicada | `{ "op": "replace", "path": "/price", "value": 9.99 }` |
| move      | Mueve un valor de una ruta a otra | `{ "op": "move", "from": "/name", "path": "/stock" }` |
| copy      | Copia un valor de una ruta a otra | `{ "op": "copy", "from": "/price", "path": "/stock" }` |
| test      | Verifica que el valor en la ruta sea igual al indicado | `{ "op": "test", "path": "/stock", "value": 10 }` |

### Ejemplo combinado de operaciones

```json
[
  { "op": "test", "path": "/stock", "value": 10 },
  { "op": "replace", "path": "/stock", "value": 20 }
]
