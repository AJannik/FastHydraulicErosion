This was done as part of my bachelor thesis in applied computer science at Ravensburg-Weingarten University. My thesis is included in the repository

The implementation uses Unity, mostly for rendering and user interaction. The underlying algorithm is hardware independant due to using GPGPU. 

This Project implements a hydraulic erosion algorithm for heightmaps based on the paper https://ieeexplore.ieee.org/abstract/document/4392715. The algorithm runs via Compute Shaders on the GPU for improved performance.

Digital landscapes are needed in many software solutions like games, simulations and movies.
These landscapes often are procedurally generated by computer algorithms or created by
scanning real world terrain data. To improve such terrain data sets, natural phenomena can
be simulated. A very common phenomena is hydraulic erosion. This work looks at a hydraulic
erosion algorithm based on the virtual pipes model and shallow-water equations. The model
is being reevaluated for application on modern computer systems, as it forms the basis for
other more advanced models. The resulting terrain achieves visually more realistic terrains
but has some artifacts and unwanted features. The model can achieve real-time execution
for large terrain data sets and can be interacted with during the simulation through diﬀerent
parameters. This is being achieved by utilising parallelisation on current graphics hardware.
