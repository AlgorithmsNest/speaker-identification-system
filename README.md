
# Speaker Identification System


## Project Overview


Our goal is to build a speaker recognition/identification system, which can be able to identify a person from the characteristics of his/her voice. In other words, we want to know who is speaking rather than recognizing what is being said.

## User Enrollment:

In this phase, a new user is introduced to the system. We create a unique template for the user that will be used later during the identification phase. For example, a UI form will collect the user’s name and voice recording, storing this data as a voice template in the database for future recognition.

![](https://i.postimg.cc/kgY9Yy3S/Enrollment.png)

## User Identification: 

In this phase, we aim to identify an unknown user. The process begins by building a sequence from the user’s input. Next, we compare this new input sequence against a database of stored voice templates to find the best match.

![](https://i.postimg.cc/qq2M96XT/speaker-identification.png)

To improve the matching process, we need to design a **sequence matching algorithm** that optimizes the search. This will include techniques like **pruning to limit the search paths and control the path cost (using methods such as beam search)**. Additionally, we may explore **time synchronization search** to enhance accuracy and efficiency during the matching process.


## Before Contributing 
Please ensure that you read and follow the guidelines outlined in the [CONTRIBUTING.md](CONTRIBUTING.md) before submitting any contributions.