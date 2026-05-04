
# Speaker Identification System


## Project Overview


Our goal is to build a speaker recognition/identification system, which can be able to identify a person from the characteristics of his/her voice. In other words, we want to know who is speaking rather than recognizing what is being said.

## User Enrollment:

In this phase, a new user is introduced to the system. We create a unique template for the user that will be used later during the identification phase. For example, a UI form will collect the user’s name and voice recording, storing this data as a voice template in the database for future recognition.

![](https://i.postimg.cc/kgY9Yy3S/Enrollment.png)

## User Identification: 

In this phase, we aim to identify an unknown user. The process begins by building a sequence from the user’s input. Next, we compare this new input sequence against a database of stored voice templates to find the best match.

![](https://i.postimg.cc/qq2M96XT/speaker-identification.png)

###  Matching Process & Optimization 

The core of the system relies on Dynamic Time Warping (DTW) to compare voice feature sequences that may differ in length and speaking speed. DTW aligns the input sequence with stored templates by allowing controlled stretching and compression along the time axis, ensuring robust matching despite natural variations in speech.

To make this process efficient and scalable, several optimization techniques are applied:

- Search Space Pruning (Diagonal Constraint):  
  The alignment search is restricted to a region near the diagonal, based on the assumption that input and template sequences have similar temporal structures. This reduces complexity from O(N × M) to O(N × W), where W is the allowed deviation width.

- Beam Search (Path Cost Pruning):  
  At each step, only paths with costs close to the current best are retained. Any path exceeding a defined threshold is discarded early, preventing unnecessary computations.

- Time-Synchronous Search:  
  Instead of matching the input against each template independently, all templates are processed simultaneously frame-by-frame. This enables:
  - real-time processing as input arrives  
  - global pruning across all templates  
  - early elimination of weak candidates  



---

### ⚙️ Matching Pipeline

1. Audio Preprocessing  
   Silence removal to reduce noise and irrelevant segments  

2. Feature Extraction  
   Convert audio into MFCC feature sequences (13 coefficients per frame)  

3. Sequence Alignment  
   Apply DTW to align input with each stored template  

4. Distance Computation  
   Frame-level distance computed using Euclidean distance  

5. Optimization  
   Apply pruning and beam search during alignment  

6. Decision  
   Select the speaker with the minimum total alignment cost  


## Contributors
- [Sama Khaled Ibrahim](https://github.com/SamaElkisarly)
- [Nrmeen Araby Elbarbary](https://github.com/NrmeenAraby)
- [Omnia Salah Mahmoud](https://github.com/Matata2020)
- [Mahmoud Mohamed Hussein](https://github.com/mahmouddmohammed)
- [Nayera Ahmed Shafik](https://github.com/Nayeraneru)
- [Youssef Mohsen Reda](https://github.com/Youssef-Mohsen)
