VAR Author = "Hunot"

=== Hub ===
#auto
Would you mind giving me a pinch? You're so cute, I must be dreaming.

* Talk
    -> Talk
* Flirt
    -> Flirt
* Insult
    -> Insult
->END

=== Talk ===
#auto
...
* Are you… human?
//mad
    {Author}: Ofc I am human! Can’t you see my human face and my tshirt? That’s just a rude thing to ask!
    -> Hub

* You seem different from the others.
//flustered
    {Author}: Well ofc I am my mom always told me I was unique. I wouldn’t mind showing you my very unique human traits.
    -> Hub

* It’s so good to find another human in this place, how did you end up here? 
//neutral
    {Author}: I have always been here just like you, we are all humans silly!
    -> Hub
->END

=== Flirt ===
#auto
...
* On a scale of 1 to 10, you're a 9 … because I'm the 1 you need.
//flustered
    {Author}: Wow a 9 is very close to 10 so that must be a good score! I am such an handsome human male!
    -> Hub

* I can't tell if that was an earthquake, or if you just seriously rocked my world.
//flustered
    {Author}: Wow you are good at this flirting thing, I like it keep talking like that and you will melt all three of my hearts- I mean my one and only heart
    -> Hub

* You are a very attractive human male who is definitely not lying about his identity.
//turns into entity
    {Author}:Well ofc I am, I love being a human male with very humanly traits
    -> HubEntity
->END


=== Insult ===
#auto
...
* You look like trouble...
//flustered
    {Author}: And you're looking a little sick, you must be suffering from lack of Vitamin Me bb <3
    -> Hub

* You are a walking red flag, I bet that there isn’t a single thought inside that head of yours.
//mad
    {Author}: For your information I am a VERY intelligent human being, I just don’t think you would be able to keep up with my intellectual thoughts
    -> Hub

* HAHAHAH your t-shirt is so ugly and fake did you know it’s written #1 and not 1#? You are so bad at faking being human.
//turns into entity
    {Author}:Fine! You win, you are right! I am Hunot and this is how I really look like. I hope you are happy now...
    -> HubEntity
->END
    
=== HubEntity ===
#auto
Fine! You win, you are right! I am Hunot and this is how I really look like. I hope you are happy now...

* Talk
    -> TalkEntity
* Flirt
    -> FlirtEntity
* Insult
    -> InsultEntity
->END
    
=== TalkEntity ===
#auto
...
* So....why did you fake being a human?
//mad
    {Author}: God forbid a guy has hobbies! There’s not much to do here and I like tricking the entities here... they aren’t very intelligent.
    -> HubEntity

* You seem different...
//neutral
    {Author}: No shit, Sherlock.
    -> HubEntity

* Can you tell me more about you?
//flustered
    {Author}: You wanna know more about me?? Well I am glad you ask, I love talking about myself! My name is Hunot and I have existed for around 100000 years, I am a very strong entity but now I am stuck in a mask I was once the most handsome being in the entire universe and now I am stuck in this form...
    -> HubEntity
->END

=== FlirtEntity ===
#auto
...
* I am sorry for being rude, I just wanted to know the real you.
//flustered
    {Author}:Aww that’s so sweet!
    -> HubEntity

* I love your long legs they are very charming. 
//neutral
    {Author}:You didn’t have anything better to compliment? You had to choose my legs?
    -> HubEntity

* Your natural face is so much better than that fake human one, I wish I could keep it with me forever...
//mad
    {Author}:Nice try, but you insulted my beautiful human disguise and hurt my feeling...I hate you....You are never getting my mask!
    -> HubEntity
->END

=== InsultEntity ===
#auto
...
* This is lowkey less disturbing than your human disguise...
//mad
    {Author}: DON’T EVER INSULT MY BEAUTIFUL DEFINED MUSCLES AND JAWLINE AGAIN! You are simply jealous cuz you are ugly and I am beautiful no matter how i look.
    -> HubEntity

* Your feet are so big that you could easily step on me, you look like big foot haha.
//flustered
    {Author}: Step on you?? I didn’t know you were kinky like that.
    -> HubEntity

* This is lowkey less disturbing than your human disguise...
//mad
    {Author}:DON’T EVER INSULT MY BEAUTIFUL DEFINED MUSCLES AND JAWLINE AGAIN! You are simply jealous cuz you are ugly and I am beautiful no matter how i look.
    -> HubEntity
->END