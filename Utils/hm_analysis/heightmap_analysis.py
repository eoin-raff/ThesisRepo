import cv2
import matplotlib.pyplot as plt
import matplotlib.image as mpimg
import numpy as np
import os
import sklearn

from mpl_toolkits.mplot3d import Axes3D
from numpy.polynomial.polynomial import polyfit
from os import listdir
from sklearn import decomposition
from sklearn.cluster import KMeans


def get_kmeans_clusters(image_data, k):
    kmeans = KMeans(n_clusters=k, random_state=0).fit(image_data[1])     # perform KMeans clustering on reshaped image
    mean_values = kmeans.cluster_centers_.T[0]                           # return the values of the cluster centers
    mean_values = np.sort(mean_values, axis=None)                        # sort the means from low to hight
    pic2show = kmeans.cluster_centers_[kmeans.labels_]                   # generate a segmented image
    
    # reshape the segmented image to a 2D array
    # divide by 255 to remap from uint8 (0-255) to float64 (0-1)
    cluster_pic = (pic2show.reshape(image_data[0].shape[0], image_data[0].shape[1]))/255 

    return mean_values, cluster_pic   


def split_image_clusters(img, mean_values):
    binary_images = []
    for i in range (0, len(mean_values)):
        ret,thresh = cv2.threshold(img,mean_values[i],255,cv2.THRESH_BINARY)
        #img_c = cv2.Canny(thresh, mean_values[i] - 0.01, mean_values[i] + 0.01)
        float_thresh = thresh/255
        binary_images.append(float_thresh)
    return binary_images


def box_count(Z, k):
    ''''''
    S = np.add.reduceat(
        np.add.reduceat(Z, np.arange(0, Z.shape[0], k), axis=0),
                           np.arange(0, Z.shape[1], k), axis=1)
    return len(np.where((S > 0) & (S < k*k))[0])

def plot_data_with_polynomial(N, constant, slope, axis):
    axis.plot(np.log(scale_factors), np.log(N), '.')
    axis.plot(np.log(scale_factors), constant + slope * np.log(scale_factors), '-')
    axis.ylabel('log(N)')
    axis.xlabel('log(S)')

def fractal_dimension(img, scale_factors):
    '''Using the box counting method, FD is the slope of the best fit line on the plot of the log log plot of the number of boxes and the size of boxes'''
    N = []
    for s in scale_factors:
        N.append(box_count(img, s*2))
        
    constant, slope = polyfit(np.log(scale_factors), np.log(N), 1)
    
    return [N, constant, slope]

def main():

    path = "img"
    suffix = "9.png"
    print('Looking for files ending in:' + str(suffix) + ' in directory : ' + str(path) + '\n')

    files = []
    for file in os.listdir(path):
        if file.endswith(suffix):
            files.append(file)

    print('Found ' + str(len(files)) + ' files ending with ' + str(suffix) + '.')

    image_data = []

    for f in files:
        original = cv2.imread(path + '\\' + f, 0)
        reshaped = original.reshape(original.shape[0]*original.shape[1], 1)
        image_data.append([original, reshaped])
        
    print('original shape: ' + str(image_data[0][0].shape))
    print('new shape: ' + str(image_data[0][1].shape))


    image_cluster_data = []
    for data in image_data:
        image_cluster_data.append(get_kmeans_clusters(data, 10))

    binary_images = []

    for i in range(len(image_data)):
        binary_images.append(split_image_clusters(image_data[i][0], image_cluster_data[i][0]))

    scale_factors = [2, 4, 8, 16, 32]
    N = []

    for s in scale_factors:
        N.append(box_count(binary_images[0][0], s*2))

    plot_data = []
    data = []
    for i in range (len(binary_images)):
        FD = []
        for img in binary_images[i]:
            N, constant, slope = fractal_dimension(img, scale_factors)
            FD.append(slope)
            plot_data.append([N, constant, slope])
        data.append(FD)

    pca = decomposition.PCA(n_components = 2)
    pca.fit(data)
    out = pca.transform(data)
    print(out.shape)

    plt.scatter(out[:, 0], out[:, 1])
    plt.show


if __name__ == "__main__":
    main()