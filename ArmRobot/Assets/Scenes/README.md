# Escenas Brazo Robot

En el proyecto del brazo robot nos encontramos con 3 tipos de escenas distintas

- La escena original del proyecto de github del brazo robot en el que el objetivo era que el robot tocase el cubo
- Las escenas del brazo robot en la mesa. En estas el objetivo es el agarre de la pieza y su traslado.
- Las escenas multiagente. En las que varios agentes distintos colaboran para lograr un objetivo.
-- Por el momento unicamente se tiene una escena de este tipo, en la que existe un agente que es una plataforma movil sobre la que se encuentra el brazo robot y debe mover el robot de un lado a otro para agarrar el cubo y despositarlo en la zona objetivo.


Para poder entrenar estos entornos realizamos el siguiente comando

```sh
mlagents-learn <RUTA AL ARCHIVO DE CONFIGURACION> --run-id=<ID DEL ENTRENAMIENTO>
```

Se puede añadir ```--force``` para reiniciar el entrenamiento de ese id o ```--resume``` para retomar el entrenamiento