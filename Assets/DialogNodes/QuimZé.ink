VAR Author = "Quim Zé"
-> start

=== start ===
#auto
Hello matey!.. How you like me hat?.
-> Hub
=== Hub ===

+ Talk
    -> Talk
+ Flirt
    -> Flirt
+ Insult
    -> Insult


=== Talk ===
#auto
...
+ Where am I? 
//neutral
    {Author}: Me Quim Zé...
    -> Hub

+ WHAT ARE YOU?!
//mad
    {Author}: Me... Quim Zé...
    -> Hub

+ What about your hat?
//neutral
    {Author}: Do you like me hat?!
    -> Hub


=== Flirt ===
#auto
...
+ You're lowkey fine no cap.
//mad
    {Author}: No cap, you no like me hat? :(
    -> Hub

+ I've never seen someone wear a hat as well as you.
//flustered
    {Author}: Thank you :)
    -> Hub

+ You're so sexy.
//neutral
    {Author}: What about me hat? :(
    -> Hub


=== Insult ===
#auto
...
+ You're so uninsultable!!!
//flustered
    {Author}: QuimZé knows.
    -> Hub

+ Are you some kind of italian brainrot?
//neutral
    {Author}: What's that...
    -> Hub

+ Your hat's ugly!
//mad
    {Author}: ...
    -> Hub
