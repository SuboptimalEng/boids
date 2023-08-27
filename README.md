# 🐟 Boids

Boids is an algorithm developed by Craig Reynolds in 1986. It aims to emulate the flocking behavior of birds by applying three simple rules: separation, alignment, and cohesion.

## Video

- [Coding Boids (Flocking Simulation) -> https://www.youtube.com/watch?v=MSZ7nqqgVKc](https://www.youtube.com/watch?v=MSZ7nqqgVKc)

  <img src="/_thumbnails/boids-02.png">

## Notes

- completed:
  - separation, alignment, cohesion for boid simulation
  - script for updating simulation settings live
  - debug view to show show range of each phase
  - color picker and randomizer for each boid
  - custom attribute `RangeWithStep` for floats
  - script to allow camera movement in play mode
  - multiple boid prefabs (sphere, cube, triangle shader)
  - implement simulation algorithm in a compute shader
- research:
  - interactable grass
  - simulating boids in 3D
  - adding a trail for each boid
  - grouping boids based on coloring
  - how to create a debug view in shader
  - following objects and avoiding edges
  - partitioning system for boid detection
  - visualizing all boids in a single shader
  - normalizing separation + alignment for a more accurate simulation

## Resources

- [Boids by Craig Reynolds](https://www.red3d.com/cwr/boids/)
- [Ben Eater's Boids Simulation](https://eater.net/boids)
- [Daniel Shiffman's Boids Simulation](https://processing.org/examples/flocking.html)
- [Sebastian Lague's Boids Coding Adventure](https://www.youtube.com/watch?v=bqtqltqcQhw)
- [David Zulic's Custom Debug Draw Tutorial for Unity](https://medium.com/@davidzulic/unity-drawing-custom-debug-shapes-part-1-4941d3fda905)
- [Catlike Coding's Creating a Procedural Mesh Tutorial](https://catlikecoding.com/unity/tutorials/procedural-meshes/creating-a-mesh/)

## License

Shield: [![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]

This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][cc-by-nc-sa].

[![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg
