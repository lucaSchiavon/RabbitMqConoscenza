# RabbitMqConoscenza
degli esempi che illustrano come creare un proudcer ed un consumer

nella route del progetto a fianco al file sln si trovano degli appunti (RabbitMqTrainigCourse.docx) che spiegano a caratteri generali cosa è rabbit mq e le sue caratteristiche principali

gli esempi sono strtturati in questo modo:

PRODUCER
in program.cs abbiamo:
creazione producer modo semplice: (senza l'uso della libreria, direttamente sulla pagina)
creazione producer exchange di tipo direct e binding  con indirizzamento mediante routingKey (senza l'uso della libreria di questa soluzione, direttamente sulla pagina)
Creazione producer con libreria NextStep.RabbitMQ

CONSUMER
in program.cs abbiamo:
creazione di una sottoscrizione (senza l'uso di librerie, direttamente sulla pagina) 
Creazione sottoscrizione con libreria NextStep.RabbitMQ